﻿using AGS.API;

namespace AGS.Engine
{
    public class AGSSliderComponent : AGSComponent, ISliderComponent
    {
        private bool _isSliding;
        private IObject _graphics, _handleGraphics;
        private ILabel _label;
        private float _minValue, _maxValue, _value;
        private SliderDirection _direction;
        private IEntity _entity;

        private readonly IFocusedUI _focus;
        private readonly IInput _input;
        private readonly IGameState _state;
        private readonly IGameEvents _gameEvents;
        private IBoundingBoxComponent _boundingBox;
        private IDrawableInfo _drawableInfo;
        private IInObjectTree _tree;
        private IVisibleComponent _visible;
        private IEnabledComponent _enabled;

        public AGSSliderComponent(IGameState state, IInput input, IGameEvents gameEvents, IFocusedUI focus)
        {
            _focus = focus;
            _state = state;
            _input = input;
            _gameEvents = gameEvents;
            AllowKeyboardControl = true;
            OnValueChanged = new AGSEvent<SliderValueEventArgs>();
            OnValueChanging = new AGSEvent<SliderValueEventArgs>();
            input.KeyUp.Subscribe(onKeyUp);
        }

        public override void Init(IEntity entity)
        {
            base.Init(entity);
            _entity = entity;
            var graphics = Graphics;
            if (graphics != null)
                graphics.Bind<IBoundingBoxComponent>(c => _boundingBox = c, _ => _boundingBox = null);
            entity.Bind<IDrawableInfo>(c => _drawableInfo = c, _ => _drawableInfo = null);
            entity.Bind<IInObjectTree>(c => _tree = c, _ => _tree = null);
            entity.Bind<IVisibleComponent>(c => _visible = c, _ => _visible = null);
            entity.Bind<IEnabledComponent>(c => _enabled = c, _ => _enabled = null);
            entity.Bind<IUIEvents>(c => c.LostFocus.Subscribe(onLostFocus), c => c.LostFocus.Unsubscribe(onLostFocus));
            _gameEvents.OnRepeatedlyExecute.Subscribe(onRepeatedlyExecute);
        }

        public IObject Graphics
        {
            get
            {
                return _graphics;
            }
            set
            {
                updateGraphics(_graphics, value, -50f);
                _graphics = value;
                if (value != null) value.Bind<IBoundingBoxComponent>(c => { _boundingBox = c; c.OnBoundingBoxesChanged.Subscribe(refresh); },
                                                                     c => { _boundingBox = null; c.OnBoundingBoxesChanged.Unsubscribe(refresh); });
                refresh();
            }
        }

        public IObject HandleGraphics
        {
            get
            {
                return _handleGraphics;
            }
            set
            {
                updateGraphics(_handleGraphics, value, -100f);
                _handleGraphics = value;
                refresh();
            }
        }

        public ILabel Label
        {
            get { return _label; }
            set
            {
                updateGraphics(_label, value, -100f);
                _label = value;
                setText();
            }
        }

        public float MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if (_minValue == value) return;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                _minValue = value;
                refresh();
            }
        }

        public float MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                if (_maxValue == value) return;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
                _maxValue = value;
                refresh();
            }
        }

        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!setValue(value)) return;
                onValueChanged();
            }
        }

        public SliderDirection Direction
        {
            get
            {
                return _direction;
            }
            set
            {
                _direction = value;
                refresh();
            }
        }

        public bool AllowKeyboardControl { get; set; }

        public IEvent<SliderValueEventArgs> OnValueChanged { get; private set; }

        public IEvent<SliderValueEventArgs> OnValueChanging { get; private set; }

        public override void Dispose()
        {
            _gameEvents.OnRepeatedlyExecute.Unsubscribe(onRepeatedlyExecute);
            if (_graphics != null) _graphics.Dispose();
            if (_handleGraphics != null) _handleGraphics.Dispose();
            if (_label != null) _label.Dispose();
        }

        private void onLostFocus(MouseButtonEventArgs args)
        {
            if (args.ClickedEntity != null && (args.ClickedEntity == Graphics || args.ClickedEntity == HandleGraphics
                                               || args.ClickedEntity == Label)) return;
            if (_focus.HasKeyboardFocus == _entity) _focus.HasKeyboardFocus = null;
        }

        private void onKeyUp(KeyboardEventArgs args)
        {
            if (_focus.HasKeyboardFocus != _entity || !AllowKeyboardControl) return;
            float smallStep = (MaxValue - MinValue) / 100f;
            float bigStep = (MaxValue - MinValue) / 5f;
            switch (args.Key)
            {
                case Key.Up:
                    if (isHorizontal()) return;
                    if (Direction == SliderDirection.BottomToTop) Value += smallStep;
                    else Value -= smallStep;
                    break;
                case Key.Down:
					if (isHorizontal()) return;
                    if (Direction == SliderDirection.BottomToTop) Value -= smallStep;
                    else Value += smallStep;
					break;
                case Key.Right:
					if (!isHorizontal()) return;
                    if (Direction == SliderDirection.LeftToRight) Value += smallStep;
                    else Value -= smallStep;
					break;
                case Key.Left:
					if (!isHorizontal()) return;
                    if (Direction == SliderDirection.LeftToRight) Value -= smallStep;
                    else Value += smallStep;
					break;
                case Key.PageUp:
                    if (Direction == SliderDirection.BottomToTop || Direction == SliderDirection.LeftToRight) Value += bigStep;
                    else Value -= bigStep;
                    break;
                case Key.PageDown:
					if (Direction == SliderDirection.BottomToTop || Direction == SliderDirection.LeftToRight) Value -= bigStep;
					else Value += bigStep;
                    break;
                case Key.Home:
                    if (Direction == SliderDirection.BottomToTop || Direction == SliderDirection.LeftToRight) Value = MinValue;
                    else Value = MaxValue;
                    break;
                case Key.End:
                    if (Direction == SliderDirection.BottomToTop || Direction == SliderDirection.LeftToRight) Value = MaxValue;
                    else Value = MinValue;
                    break;
                default:
                    break;
            }
        }

		private void updateGraphics(IObject oldGraphics, IObject newGraphics, float z)
		{
			if (oldGraphics != null)
			{
				_state.UI.Remove(oldGraphics);
				oldGraphics.TreeNode.SetParent(null);
			}
			if (newGraphics == null) return;
            var drawableInfo = _drawableInfo;
            if (drawableInfo != null) newGraphics.RenderLayer = drawableInfo.RenderLayer;
			newGraphics.Z = z;
            var tree = _tree;
            if (tree != null) newGraphics.TreeNode.SetParent(tree.TreeNode);
			_state.UI.Add(newGraphics);
		}

		private void onRepeatedlyExecute()
		{
            var boundingBox = _boundingBox;
            if (boundingBox == null) return;
            var boundingBoxes = boundingBox.GetBoundingBoxes(_state.Viewport);
            var visible = _visible;
            var enabled = _enabled;
            if (visible == null || !visible.Visible || enabled == null || !enabled.Enabled || 
                enabled.ClickThrough || boundingBoxes == null || 
                (!_input.LeftMouseButtonDown && !_input.IsTouchDrag) || Graphics == null || 
                Graphics.GetBoundingBoxes(_state.Viewport) == null || !Graphics.CollidesWith(
                    _input.MousePosition.XMainViewport, _input.MousePosition.YMainViewport, _state.Viewport) || 
                HandleGraphics == null)
			{
				if (_isSliding)
				{
					_isSliding = false;
					onValueChanged();
				}
				return;
			}
            _focus.HasKeyboardFocus = _entity;
			_isSliding = true;
            if (isHorizontal()) setValue(getSliderValue(MathUtils.Clamp(_input.MousePosition.XMainViewport - boundingBoxes.HitTestBox.MinX, 
                                                                      0f, boundingBoxes.HitTestBox.Width), boundingBoxes));
            else setValue(getSliderValue(MathUtils.Clamp(_input.MousePosition.YMainViewport - boundingBoxes.HitTestBox.MinY
                                                         , 0f, boundingBoxes.HitTestBox.Height), boundingBoxes));
		}

		private void refresh()
		{
            var boundingBox = _boundingBox;
            if (boundingBox == null) return;
            var boundingBoxes = boundingBox.GetBoundingBoxes(_state.Viewport);
            if (boundingBoxes == null || HandleGraphics == null) return;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (MinValue == MaxValue) return;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

            var handlePos = getHandlePos(Value, boundingBoxes);
            if (isHorizontal()) HandleGraphics.X = MathUtils.Clamp(handlePos, 0f, boundingBoxes.RenderBox.Width);
            else HandleGraphics.Y = MathUtils.Clamp(handlePos, 0f, boundingBoxes.RenderBox.Height);
			setText();
		}

        private float getSliderValue(float handlePos, AGSBoundingBoxes boundingBoxes)
		{
            float min = isReverse() ? MaxValue : MinValue;
            float max = isReverse() ? MinValue : MaxValue;
            return MathUtils.Lerp(0f, min, isHorizontal() ? 
                                  boundingBoxes.HitTestBox.Width : boundingBoxes.HitTestBox.Height, 
                                  max, handlePos);
		}

		private float getHandlePos(float value, AGSBoundingBoxes boundingBoxes)
		{
			float min = isReverse() ? MaxValue : MinValue;
			float max = isReverse() ? MinValue : MaxValue;
            return MathUtils.Lerp(min, 0f, max, isHorizontal() ? 
                                  boundingBoxes.RenderBox.Width : boundingBoxes.RenderBox.Height, 
                                  value);
		}

		private void setText()
		{
			if (_label == null) return;
			_label.Text = ((int)Value).ToString();
		}

		private bool setValue(float value)
		{
#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
            if (_value == value || value < MinValue || value > MaxValue) return false;
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
            _value = value;
            OnValueChanging.Invoke(new SliderValueEventArgs(_value));
			refresh();
            return true;
		}

		private void onValueChanged()
		{
            var args = new SliderValueEventArgs(_value);
            OnValueChanging.Invoke(args);
			OnValueChanged.Invoke(args);
		}

        private bool isHorizontal()
        {
            return _direction == SliderDirection.LeftToRight || _direction == SliderDirection.RightToLeft;
        }

        private bool isReverse()
        {
            return _direction == SliderDirection.RightToLeft || _direction == SliderDirection.TopToBottom;
        }
	}
}
