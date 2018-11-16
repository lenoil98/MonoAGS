﻿

//This class was automatically generated by a T4 template.
//Making manual changes in this class might be overridden if the template will be processed again.
//If you want to add functionality you might be able to do this via another partial class definition for this class.

using System;
using AGS.API;
using AGS.Engine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Autofac;

namespace AGS.Engine
{
    public partial class AGSCharacter : AGSEntity, ICharacter
    {
        private IHasRoomComponent _hasRoomComponent;
        private IAnimationComponent _animationComponent;
        private IInObjectTreeComponent _inObjectTreeComponent;
        private IColliderComponent _colliderComponent;
        private IVisibleComponent _visibleComponent;
        private IEnabledComponent _enabledComponent;
        private ICustomPropertiesComponent _customPropertiesComponent;
        private IDrawableInfoComponent _drawableInfoComponent;
        private IShaderComponent _shaderComponent;
        private ITranslateComponent _translateComponent;
        private IImageComponent _imageComponent;
        private IBorderComponent _borderComponent;
        private IScaleComponent _scaleComponent;
        private IRotateComponent _rotateComponent;
        private IPixelPerfectComponent _pixelPerfectComponent;
        private IModelMatrixComponent _modelMatrixComponent;
        private IBoundingBoxComponent _boundingBoxComponent;
        private IWorldPositionComponent _worldPositionComponent;
        private ISayComponent _sayComponent;
        private IWalkComponent _walkComponent;
        private IFaceDirectionComponent _faceDirectionComponent;
        private IOutfitComponent _outfitComponent;
        private IInventoryComponent _inventoryComponent;
        private IFollowComponent _followComponent;
        private IHotspotComponent _hotspotComponent;

        public AGSCharacter(string id, Resolver resolver, IOutfit outfit) : base(id, resolver)
        {
            _hasRoomComponent = AddComponent<IHasRoomComponent>();
            Bind<IHasRoomComponent>(c => _hasRoomComponent = c, _ => {});
            _animationComponent = AddComponent<IAnimationComponent>();
            Bind<IAnimationComponent>(c => _animationComponent = c, _ => {});
            _inObjectTreeComponent = AddComponent<IInObjectTreeComponent>();
            Bind<IInObjectTreeComponent>(c => _inObjectTreeComponent = c, _ => {});
            _colliderComponent = AddComponent<IColliderComponent>();
            Bind<IColliderComponent>(c => _colliderComponent = c, _ => {});
            _visibleComponent = AddComponent<IVisibleComponent>();
            Bind<IVisibleComponent>(c => _visibleComponent = c, _ => {});
            _enabledComponent = AddComponent<IEnabledComponent>();
            Bind<IEnabledComponent>(c => _enabledComponent = c, _ => {});
            _customPropertiesComponent = AddComponent<ICustomPropertiesComponent>();
            Bind<ICustomPropertiesComponent>(c => _customPropertiesComponent = c, _ => {});
            _drawableInfoComponent = AddComponent<IDrawableInfoComponent>();
            Bind<IDrawableInfoComponent>(c => _drawableInfoComponent = c, _ => {});
            _shaderComponent = AddComponent<IShaderComponent>();
            Bind<IShaderComponent>(c => _shaderComponent = c, _ => {});
            _translateComponent = AddComponent<ITranslateComponent>();
            Bind<ITranslateComponent>(c => _translateComponent = c, _ => {});
            _imageComponent = AddComponent<IImageComponent>();
            Bind<IImageComponent>(c => _imageComponent = c, _ => {});
            _borderComponent = AddComponent<IBorderComponent>();
            Bind<IBorderComponent>(c => _borderComponent = c, _ => { });
            _scaleComponent = AddComponent<IScaleComponent>();
            Bind<IScaleComponent>(c => _scaleComponent = c, _ => {});
            _rotateComponent = AddComponent<IRotateComponent>();
            Bind<IRotateComponent>(c => _rotateComponent = c, _ => {});
            _pixelPerfectComponent = AddComponent<IPixelPerfectComponent>();
            Bind<IPixelPerfectComponent>(c => _pixelPerfectComponent = c, _ => {});
            _modelMatrixComponent = AddComponent<IModelMatrixComponent>();
            Bind<IModelMatrixComponent>(c => _modelMatrixComponent = c, _ => {});
            _boundingBoxComponent = AddComponent<IBoundingBoxComponent>();
            Bind<IBoundingBoxComponent>(c => _boundingBoxComponent = c, _ => {});
            _worldPositionComponent = AddComponent<IWorldPositionComponent>();
            Bind<IWorldPositionComponent>(c => _worldPositionComponent = c, _ => {});
            _faceDirectionComponent = AddComponent<IFaceDirectionComponent>();
            Bind<IFaceDirectionComponent>(c => _faceDirectionComponent = c, _ => {});
            _outfitComponent = AddComponent<IOutfitComponent>();
            Bind<IOutfitComponent>(c => _outfitComponent = c, _ => {});
            _inventoryComponent = AddComponent<IInventoryComponent>();
            Bind<IInventoryComponent>(c => _inventoryComponent = c, _ => {});
            _followComponent = AddComponent<IFollowComponent>();
            Bind<IFollowComponent>(c => _followComponent = c, _ => {});
            _hotspotComponent = AddComponent<IHotspotComponent>();
            Bind<IHotspotComponent>(c => _hotspotComponent = c, _ => {});
			beforeInitComponents(resolver, outfit);
            InitComponents();
            afterInitComponents(resolver, outfit);
            }

        public string Name { get { return ID; } }
        public bool AllowMultiple { get { return false; } }
        public IEntity Entity { get => this; }
        public Type RegistrationType { get => typeof(IEntity); }
        public void Init(IEntity entity, Type registrationType) { }

        public override string ToString()
        {
            return string.Format("{0} ({1})", ID ?? "", GetType().Name);
        }

        partial void beforeInitComponents(Resolver resolver, IOutfit outfit);
		partial void afterInitComponents(Resolver resolver, IOutfit outfit);

        #region IHasRoomComponent implementation

        public IRoom Room 
        {  
            get { return _hasRoomComponent.Room; } 
        }

        public IRoom PreviousRoom 
        {  
            get { return _hasRoomComponent.PreviousRoom; } 
        }

        public IBlockingEvent OnRoomChanged 
        {  
            get { return _hasRoomComponent.OnRoomChanged; } 
        }

        #endregion

        #region INotifyPropertyChanged implementation

        #endregion

        #region IAnimationComponent implementation

        public IAnimation Animation 
        {  
            get { return _animationComponent.Animation; } 
        }

        public IBlockingEvent OnAnimationStarted 
        {  
            get { return _animationComponent.OnAnimationStarted; } 
        }

        public void StartAnimation(IAnimation animation)
        {
            _animationComponent.StartAnimation(animation);
        }

        public AnimationCompletedEventArgs Animate(IAnimation animation)
        {
            return _animationComponent.Animate(animation);
        }

        public Task<AnimationCompletedEventArgs> AnimateAsync(IAnimation animation)
        {
            return _animationComponent.AnimateAsync(animation);
        }

        #endregion

        #region IInObjectTreeComponent implementation

        #endregion

        #region IInTree<IObject> implementation

        public ITreeNode<IObject> TreeNode 
        {  
            get { return _inObjectTreeComponent.TreeNode; } 
        }

        #endregion

        #region IColliderComponent implementation

        public Nullable<PointF> CenterPoint 
        {  
            get { return _colliderComponent.CenterPoint; } 
        }

        public Boolean CollidesWith(Single x, Single y, IViewport viewport)
        {
            return _colliderComponent.CollidesWith(x, y, viewport);
        }

        #endregion

        #region IVisibleComponent implementation

        public Boolean Visible 
        {  
            get { return _visibleComponent.Visible; }  
            set { _visibleComponent.Visible = value; } 
        }

        public Boolean UnderlyingVisible 
        {  
            get { return _visibleComponent.UnderlyingVisible; } 
        }

        #endregion

        #region IEnabledComponent implementation

        public Boolean Enabled 
        {  
            get { return _enabledComponent.Enabled; }  
            set { _enabledComponent.Enabled = value; } 
        }

        public Boolean UnderlyingEnabled 
        {  
            get { return _enabledComponent.UnderlyingEnabled; } 
        }

        public Boolean ClickThrough 
        {  
            get { return _enabledComponent.ClickThrough; }  
            set { _enabledComponent.ClickThrough = value; } 
        }

        #endregion

        #region ICustomPropertiesComponent implementation

        public ICustomProperties Properties 
        {  
            get { return _customPropertiesComponent.Properties; } 
        }

        #endregion

        #region IDrawableInfoComponent implementation

        public IRenderLayer RenderLayer 
        {  
            get { return _drawableInfoComponent.RenderLayer; }  
            set { _drawableInfoComponent.RenderLayer = value; } 
        }

        public Boolean IgnoreViewport 
        {  
            get { return _drawableInfoComponent.IgnoreViewport; }  
            set { _drawableInfoComponent.IgnoreViewport = value; } 
        }

        public Boolean IgnoreScalingArea 
        {  
            get { return _drawableInfoComponent.IgnoreScalingArea; }  
            set { _drawableInfoComponent.IgnoreScalingArea = value; } 
        }

        #endregion

        #region IShaderComponent implementation

        public IShader Shader 
        {  
            get { return _shaderComponent.Shader; }  
            set { _shaderComponent.Shader = value; } 
        }

        #endregion

        #region ITranslateComponent implementation

        #endregion

        #region ITranslate implementation

        public Position Position 
        {  
            get { return _translateComponent.Position; }  
            set { _translateComponent.Position = value; } 
        }

        public Single X 
        {  
            get { return _translateComponent.X; }  
            set { _translateComponent.X = value; } 
        }

        public Single Y 
        {  
            get { return _translateComponent.Y; }  
            set { _translateComponent.Y = value; } 
        }

        public Single Z 
        {  
            get { return _translateComponent.Z; }  
            set { _translateComponent.Z = value; } 
        }

        #endregion

        #region IImageComponent implementation

        public ISprite CurrentSprite 
        {  
            get { return _imageComponent.CurrentSprite; } 
        }

        public ISpriteProvider SpriteProvider 
        {  
            get { return _imageComponent.SpriteProvider; }  
            set { _imageComponent.SpriteProvider = value; } 
        }

        #endregion

        #region IBorderComponent implementation

        public IBorderStyle Border
        {
            get { return _borderComponent.Border; }
            set { _borderComponent.Border = value; }
        }

        #endregion

        #region IHasImage implementation

        public Byte Opacity 
        {  
            get { return _imageComponent.Opacity; }  
            set { _imageComponent.Opacity = value; } 
        }

        public Color Tint 
        {  
            get { return _imageComponent.Tint; }  
            set { _imageComponent.Tint = value; } 
        }

        public Vector4 Brightness 
        {  
            get { return _imageComponent.Brightness; }  
            set { _imageComponent.Brightness = value; } 
        }

        public PointF Pivot 
        {  
            get { return _imageComponent.Pivot; }  
            set { _imageComponent.Pivot = value; } 
        }

        public IImage Image 
        {  
            get { return _imageComponent.Image; }  
            set { _imageComponent.Image = value; } 
        }

        public bool IsImageVisible
        {
            get { return _imageComponent.IsImageVisible; }
            set { _imageComponent.IsImageVisible = value; }
        }

        #endregion

        #region IScaleComponent implementation

        #endregion

        #region IScale implementation

        public Single Height 
        {  
            get { return _scaleComponent.Height; } 
        }

        public Single Width 
        {  
            get { return _scaleComponent.Width; } 
        }

        public Single ScaleX 
        {  
            get { return _scaleComponent.ScaleX; }  
            set { _scaleComponent.ScaleX = value; } 
        }

        public Single ScaleY 
        {  
            get { return _scaleComponent.ScaleY; }  
            set { _scaleComponent.ScaleY = value; } 
        }

        public PointF Scale 
        {  
            get { return _scaleComponent.Scale; }  
            set { _scaleComponent.Scale = value; } 
        }

        public SizeF BaseSize 
        {  
            get { return _scaleComponent.BaseSize; }  
            set { _scaleComponent.BaseSize = value; } 
        }

        public void ResetScale()
        {
            _scaleComponent.ResetScale();
        }

        public void ResetScale(Single initialWidth, Single initialHeight)
        {
            _scaleComponent.ResetScale(initialWidth, initialHeight);
        }

        public void ScaleTo(Single width, Single height)
        {
            _scaleComponent.ScaleTo(width, height);
        }

        public void FlipHorizontally()
        {
            _scaleComponent.FlipHorizontally();
        }

        public void FlipVertically()
        {
            _scaleComponent.FlipVertically();
        }

        #endregion

        #region IRotateComponent implementation

        #endregion

        #region IRotate implementation

        public Single Angle 
        {  
            get { return _rotateComponent.Angle; }  
            set { _rotateComponent.Angle = value; } 
        }

        #endregion

        #region IPixelPerfectComponent implementation

        public Boolean IsPixelPerfect 
        {  
            get { return _pixelPerfectComponent.IsPixelPerfect; }  
            set { _pixelPerfectComponent.IsPixelPerfect = value; } 
        }

        #endregion

        #region IPixelPerfectCollidable implementation

        public IArea PixelPerfectHitTestArea 
        {  
            get { return _pixelPerfectComponent.PixelPerfectHitTestArea; } 
        }

        #endregion

        #region IModelMatrixComponent implementation

        public IBlockingEvent OnMatrixChanged 
        {  
            get { return _modelMatrixComponent.OnMatrixChanged; } 
        }

        public ILockStep ModelMatrixLockStep 
        {  
            get { return _modelMatrixComponent.ModelMatrixLockStep; } 
        }

        public ref ModelMatrices GetModelMatrices()
        {
            return ref _modelMatrixComponent.GetModelMatrices();
        }

        #endregion

        #region IBoundingBoxComponent implementation

        public AGSBoundingBox WorldBoundingBox 
        {  
            get { return _boundingBoxComponent.WorldBoundingBox; } 
        }

        public IBlockingEvent OnBoundingBoxesChanged 
        {  
            get { return _boundingBoxComponent.OnBoundingBoxesChanged; } 
        }

        public ILockStep BoundingBoxLockStep 
        {  
            get { return _boundingBoxComponent.BoundingBoxLockStep; } 
        }

        public AGSBoundingBoxes GetBoundingBoxes(IViewport viewport)
        {
            return _boundingBoxComponent.GetBoundingBoxes(viewport);
        }

        #endregion

        #region IWorldPositionComponent implementation

        public Single WorldX 
        {  
            get { return _worldPositionComponent.WorldX; } 
        }

        public Single WorldY 
        {  
            get { return _worldPositionComponent.WorldY; } 
        }

        public PointF WorldXY
        {
            get { return _worldPositionComponent.WorldXY; }
        }

        #endregion

        #region ISayComponent implementation

        public ISayConfig SpeechConfig 
        {  
            get { return _sayComponent.SpeechConfig; } 
        }

        public IBlockingEvent<BeforeSayEventArgs> OnBeforeSay 
        {  
            get { return _sayComponent.OnBeforeSay; } 
        }

        public Task SayAsync(String text, PointF? textPosition = null, PointF? portraitPosition = null)
        {
            return _sayComponent.SayAsync(text, textPosition, portraitPosition);
        }

        #endregion

        #region IWalkComponent implementation

        public PointF WalkStep 
        {  
            get { return _walkComponent.WalkStep; }  
            set { _walkComponent.WalkStep = value; } 
        }

        public Boolean AdjustWalkSpeedToScaleArea 
        {  
            get { return _walkComponent.AdjustWalkSpeedToScaleArea; }  
            set { _walkComponent.AdjustWalkSpeedToScaleArea = value; } 
        }

        public Boolean MovementLinkedToAnimation 
        {  
            get { return _walkComponent.MovementLinkedToAnimation; }  
            set { _walkComponent.MovementLinkedToAnimation = value; } 
        }

        public Boolean IsWalking 
        {  
            get { return _walkComponent.IsWalking; } 
        }

        public Position WalkDestination 
        {  
            get { return _walkComponent.WalkDestination; } 
        }

        public Boolean DebugDrawWalkPath 
        {  
            get { return _walkComponent.DebugDrawWalkPath; }  
            set { _walkComponent.DebugDrawWalkPath = value; } 
        }

        public Task<Boolean> WalkAsync(Position position, bool walkAnywhere = false)
        {
            return _walkComponent.WalkAsync(position, walkAnywhere);
        }

        public Task<Boolean> WalkStraightAsync(Position position)
        {
            return _walkComponent.WalkStraightAsync(position);
        }

        public Task StopWalkingAsync()
        {
            return _walkComponent.StopWalkingAsync();
        }

        public void PlaceOnWalkableArea()
        {
            _walkComponent.PlaceOnWalkableArea();
        }

        #endregion

        #region IFaceDirectionComponent implementation

        public Direction Direction 
        {  
            get { return _faceDirectionComponent.Direction; } 
        }

        public IDirectionalAnimation CurrentDirectionalAnimation 
        {  
            get { return _faceDirectionComponent.CurrentDirectionalAnimation; }  
            set { _faceDirectionComponent.CurrentDirectionalAnimation = value; } 
        }

        public void FaceDirection(Direction direction)
        {
            _faceDirectionComponent.FaceDirection(direction);
        }

        public Task FaceDirectionAsync(Direction direction)
        {
            return _faceDirectionComponent.FaceDirectionAsync(direction);
        }

        public void FaceDirection(IObject obj)
        {
            _faceDirectionComponent.FaceDirection(obj);
        }

        public Task FaceDirectionAsync(IObject obj)
        {
            return _faceDirectionComponent.FaceDirectionAsync(obj);
        }

        public void FaceDirection(Single x, Single y)
        {
            _faceDirectionComponent.FaceDirection(x, y);
        }

        public Task FaceDirectionAsync(Single x, Single y)
        {
            return _faceDirectionComponent.FaceDirectionAsync(x, y);
        }

        public void FaceDirection(Single fromX, Single fromY, Single toX, Single toY)
        {
            _faceDirectionComponent.FaceDirection(fromX, fromY, toX, toY);
        }

        public Task FaceDirectionAsync(Single fromX, Single fromY, Single toX, Single toY)
        {
            return _faceDirectionComponent.FaceDirectionAsync(fromX, fromY, toX, toY);
        }

        #endregion

        #region IOutfitComponent implementation

        public IOutfit Outfit 
        {  
            get { return _outfitComponent.Outfit; }  
            set { _outfitComponent.Outfit = value; } 
        }

        #endregion

        #region IInventoryComponent implementation

        public IInventory Inventory 
        {  
            get { return _inventoryComponent.Inventory; }  
            set { _inventoryComponent.Inventory = value; } 
        }

        #endregion

        #region IFollowComponent implementation

        public IObject TargetBeingFollowed 
        {  
            get { return _followComponent.TargetBeingFollowed; } 
        }

        public void Follow(IObject obj, IFollowSettings settings)
        {
            _followComponent.Follow(obj, settings);
        }

        public Task StopFollowingAsync()
        {
            return _followComponent.StopFollowingAsync();
        }

        #endregion

        #region IHotspotComponent implementation

        public IInteractions Interactions 
        {  
            get { return _hotspotComponent.Interactions; } 
        }

        public Nullable<PointF> WalkPoint 
        {  
            get { return _hotspotComponent.WalkPoint; }  
            set { _hotspotComponent.WalkPoint = value; } 
        }

        public Boolean DisplayHotspot 
        {  
            get { return _hotspotComponent.DisplayHotspot; }  
            set { _hotspotComponent.DisplayHotspot = value; } 
        }

        #endregion
    }
}

