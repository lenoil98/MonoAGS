﻿using System;
using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace AGS.Editor
{
    public class GameDebugView
    {
        private readonly IRenderLayer _layer;
        private readonly IGame _game;
        private readonly GameDebugTree _debugTree;
        private readonly GameDebugDisplayList _displayList;
        private readonly InspectorPanel _inspector;
        private readonly IInput _input;
        private readonly KeyboardBindings _keyboardBindings;
        private const string _panelId = "Game Debug Tree Panel";
        private IPanel _panel;
        private ISplitPanelComponent _splitPanel;
        private IDebugTab _currentTab;
        private IButton _panesButton;

        public GameDebugView(IGame game, KeyboardBindings keyboardBindings)
        {
            _game = game;
            _keyboardBindings = keyboardBindings;
            _layer = new AGSRenderLayer(AGSLayers.UI.Z - 1, independentResolution: new Size(1800, 1200));
            _inspector = new InspectorPanel(game, _layer);
            _debugTree = new GameDebugTree(game, _layer, _inspector);
            _displayList = new GameDebugDisplayList(game, _layer);
            _input = game.Input;
            keyboardBindings.OnKeyboardShortcutPressed.Subscribe(onShortcutKeyPressed);
            game.Events.OnRepeatedlyExecute.Subscribe(onRepeatedlyExecute);
        }

        public bool Visible => _panel.Visible;

        public void Load()
        {
            const float headerHeight = 50f;
            const float borderWidth = 3f;
            IGameFactory factory = _game.Factory;
            _panel = factory.UI.GetPanel(_panelId, _layer.IndependentResolution.Value.Width / 4f, _layer.IndependentResolution.Value.Height,
                                                     1f, _layer.IndependentResolution.Value.Height / 2f);
            _panel.Pivot = new PointF(0f, 0.5f);
            _panel.Visible = false;
            _panel.Tint = GameViewColors.Panel;
            _panel.Border = AGSBorders.SolidColor(GameViewColors.Border, borderWidth, hasRoundCorners: true);
            _panel.RenderLayer = _layer;
            _panel.ClickThrough = false;
            _game.State.FocusedUI.CannotLoseFocus.Add(_panelId);

            var headerLabel = factory.UI.GetLabel("GameDebugTreeLabel", "Game Debug", _panel.Width, headerHeight, 0f, _panel.Height - headerHeight,
                                      _panel, new AGSTextConfig(alignment: Alignment.MiddleCenter, autoFit: AutoFit.TextShouldFitLabel));
            headerLabel.Tint = Colors.Transparent;
            headerLabel.Border = _panel.Border;
            headerLabel.RenderLayer = _layer;

            var xButton = factory.UI.GetButton("GameDebugTreeCloseButton", (IAnimation)null, null, null, 0f, _panel.Height - headerHeight + 5f, _panel, "X",
                                               new AGSTextConfig(factory.Graphics.Brushes.LoadSolidBrush(Colors.Red),
                                                                 autoFit: AutoFit.TextShouldFitLabel, alignment: Alignment.MiddleCenter),
                                                                 width: 40f, height: 40f);
            xButton.Pivot = new PointF();
            xButton.RenderLayer = _layer;
            xButton.Tint = Colors.Transparent;
            xButton.MouseEnter.Subscribe(_ => xButton.TextConfig = AGSTextConfig.ChangeColor(xButton.TextConfig, GameViewColors.HoveredText, GameViewColors.HoveredText, 0.3f));
            xButton.MouseLeave.Subscribe(_ => xButton.TextConfig = AGSTextConfig.ChangeColor(xButton.TextConfig, Colors.Red, Colors.Transparent, 0f));
            xButton.MouseClicked.Subscribe(_ => Hide());

            _panesButton = factory.UI.GetButton("GameDebugViewPanesButton", (IAnimation)null, null, null, _panel.Width, xButton.Y, _panel, "Display List",
                                                   new AGSTextConfig(autoFit: AutoFit.TextShouldFitLabel, alignment: Alignment.MiddleRight),
                                                   width: 120f, height: 40f);
            _panesButton.Pivot = new PointF(1f, 0f);
            _panesButton.RenderLayer = _layer;
            _panesButton.Tint = GameViewColors.Button;
            _panesButton.MouseEnter.Subscribe(_ => _panesButton.TextConfig = AGSTextConfig.ChangeColor(xButton.TextConfig, GameViewColors.HoveredText, GameViewColors.HoveredText, 0.3f));
            _panesButton.MouseLeave.Subscribe(_ => _panesButton.TextConfig = AGSTextConfig.ChangeColor(xButton.TextConfig, GameViewColors.Text, Colors.Transparent, 0f));
            _panesButton.MouseClicked.SubscribeToAsync(onPaneSwitch);

            var parentPanelHeight = _panel.Height - headerHeight;
            var parentPanel = factory.UI.GetPanel("GameDebugParentPanel", _panel.Width, parentPanelHeight, 0f, parentPanelHeight, _panel);
            parentPanel.Pivot = new PointF(0f, 1f);
            parentPanel.Tint = Colors.Transparent;
            parentPanel.RenderLayer = _layer;

            var topPanel = factory.UI.GetPanel("GameDebugTopPanel", _panel.Width, parentPanelHeight / 2f, 0f, parentPanelHeight / 2f, parentPanel);
            topPanel.Pivot = new PointF(0f, 0f);
            topPanel.Tint = Colors.Transparent;
            topPanel.RenderLayer = _layer;

            var bottomPanel = factory.UI.GetPanel("GameDebugBottomPanel", _panel.Width, parentPanelHeight / 2f, 0f, parentPanelHeight / 2f, parentPanel);
            bottomPanel.Pivot = new PointF(0f, 1f);
            bottomPanel.Tint = Colors.Transparent;
            bottomPanel.RenderLayer = _layer;

            _debugTree.Load(topPanel);
            _displayList.Load(topPanel);
            _inspector.Load(bottomPanel);
            _currentTab = _debugTree;
            _splitPanel = parentPanel.AddComponent<ISplitPanelComponent>();
            _splitPanel.TopPanel = topPanel;
            _splitPanel.BottomPanel = bottomPanel;

            var horizSplit = _panel.AddComponent<ISplitPanelComponent>();
            horizSplit.IsHorizontal = true;
            horizSplit.TopPanel = _panel;

            _panel.GetComponent<IScaleComponent>().PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != nameof(IScaleComponent.Width)) return;
                _panesButton.X = _panel.Width;
                headerLabel.LabelRenderSize = new SizeF(_panel.Width, headerLabel.LabelRenderSize.Height);
                parentPanel.BaseSize = new SizeF(_panel.Width, parentPanel.Height);
                topPanel.BaseSize = new SizeF(_panel.Width, topPanel.Height);
                bottomPanel.BaseSize = new SizeF(_panel.Width, bottomPanel.Height);
                _currentTab.Resize();
                _inspector.Resize();
            };
        }

        public Task Show()
        {
            _panel.Visible = true;
            return _currentTab.Show();
        }

        public void Hide()
        {
            _panel.Visible = false;
            _currentTab.Hide();
        }

        private Task onPaneSwitch(MouseButtonEventArgs args)
        {
            _currentTab.Hide();
            _currentTab = (_currentTab == _debugTree) ? (IDebugTab)_displayList : _debugTree;
            _panesButton.Text = _currentTab == _debugTree ? "Display List" : "Scene Tree";
            _currentTab.Resize();
            return _currentTab.Show();
        }

        private void onShortcutKeyPressed(string action)
        {
            if (!_panel?.Visible ?? false) return;

            if (action == KeyboardBindings.Undo)
            {
                _inspector?.Inspector?.Undo();
            }
            else if (action == KeyboardBindings.Redo)
            {
                _inspector?.Inspector?.Redo();
            }
        }

        private void moveEntity(IEntity entity, float xOffset, float yOffset)
        {
            var translate = entity.GetComponent<ITranslateComponent>();
            if (translate == null) return;
            if (_input.IsKeyDown(Key.AltLeft) || _input.IsKeyDown(Key.AltRight))
            {
                xOffset /= 10f;
                yOffset /= 10f;
            }
            if (!MathUtils.FloatEquals(xOffset, 0f)) translate.X += xOffset;
            if (!MathUtils.FloatEquals(yOffset, 0f)) translate.Y += yOffset;
        }

        private void rotateEntity(IEntity entity, float angleOffset)
        {
            var rotate = entity.GetComponent<IRotateComponent>();
            if (rotate == null) return;
            if (_input.IsKeyDown(Key.AltLeft) || _input.IsKeyDown(Key.AltRight))
            {
                angleOffset /= 10f;
            }
            rotate.Angle += angleOffset;
        }

        private void onRepeatedlyExecute(IRepeatedlyExecuteEventArgs args)
        {
            var entity = _inspector.Inspector.SelectedObject as IEntity;
            if (entity == null) return;

            if (_input.IsKeyDown(Key.Down)) moveEntity(entity, 0f, -1f);
            else if (_input.IsKeyDown(Key.Up)) moveEntity(entity, 0f, 1f);

            if (_input.IsKeyDown(Key.Left)) moveEntity(entity, -1f, 0f);
            else if (_input.IsKeyDown(Key.Right)) moveEntity(entity, 1f, 0f);

            if (_input.IsKeyDown(Key.BracketLeft)) rotateEntity(entity, -1f);
            else if (_input.IsKeyDown(Key.BracketRight)) rotateEntity(entity, 1f);
        }
    }
}