﻿using System;
using AGS.API;

namespace AGS.Engine
{
    public class AGSModelMatrixComponent : AGSComponent, IModelMatrixComponent
    {
        private bool _isDirty;
        private ModelMatrices _matrices;

        private IAnimationContainer _animation;
        private IInObjectTree _tree;
        private IScaleComponent _scale;
        private ITranslateComponent _translate;
        private IRotateComponent _rotate;
        private IImageComponent _image;
        private IHasRoom _room;
        private IDrawableInfo _drawable;
        private IEntity _entity;
        private IObject _parent;
        private ISprite _sprite;
        private ICropSelfComponent _crop;

        private readonly Size _virtualResolution;
        private PointF _areaScaling;
        private SizeF? _customImageSize;
        private PointF? _customResolutionFactor;
        private readonly float? _nullFloat = null;

        public AGSModelMatrixComponent(IRuntimeSettings settings)
        {
            _isDirty = true;
            _matrices = new ModelMatrices();
            _virtualResolution = settings.VirtualResolution;
            OnMatrixChanged = new AGSEvent<object>();
        }

        public static readonly PointF NoScaling = new PointF(1f, 1f);

        public override void Init(IEntity entity)
        {
            base.Init(entity);
            _entity = entity;
            entity.Bind<IAnimationContainer>(
                c => { _animation = c; onSomethingChanged(null); },
                c => { _animation = null; onSomethingChanged(null); });
            entity.Bind<IHasRoom>(
                c => { _room = c; onSomethingChanged(null); },
                c => { _room = null; onSomethingChanged(null); });
            
            entity.Bind<IScaleComponent>(
                c => { _scale = c; c.OnScaleChanged.Subscribe(onSomethingChanged); onSomethingChanged(null); },
                c => { c.OnScaleChanged.Unsubscribe(onSomethingChanged); _scale = null; onSomethingChanged(null);});
            entity.Bind<ITranslateComponent>(
                c => { _translate = c; c.OnLocationChanged.Subscribe(onSomethingChanged); onSomethingChanged(null); },
                c => { c.OnLocationChanged.Unsubscribe(onSomethingChanged); _translate = null; onSomethingChanged(null);}
            );
            entity.Bind<IRotateComponent>(
                c => { _rotate = c; c.OnAngleChanged.Subscribe(onSomethingChanged); onSomethingChanged(null);},
                c => { c.OnAngleChanged.Unsubscribe(onSomethingChanged); _rotate = null; onSomethingChanged(null);}
            );
			entity.Bind<IImageComponent>(
                c => { _image = c; c.OnAnchorChanged.Subscribe(onSomethingChanged); onSomethingChanged(null); },
                c => { c.OnAnchorChanged.Unsubscribe(onSomethingChanged); _image = null; onSomethingChanged(null); }
			);
            entity.Bind<ICropSelfComponent>(
                c => { _crop = c; c.OnCropAreaChanged.Subscribe(onSomethingChanged); onSomethingChanged(null); },
				c => { c.OnCropAreaChanged.Unsubscribe(onSomethingChanged); _crop = null; onSomethingChanged(null); }
			);

            entity.Bind<IDrawableInfo>(
                c => 
            {
                _drawable = c;
				c.OnIgnoreScalingAreaChanged.Subscribe(onSomethingChanged);
				c.OnRenderLayerChanged.Subscribe(onSomethingChanged);
                onSomethingChanged(null);
            },c =>
            {
                c.OnIgnoreScalingAreaChanged.Unsubscribe(onSomethingChanged);
				c.OnRenderLayerChanged.Unsubscribe(onSomethingChanged);
                _drawable = null;
				onSomethingChanged(null);
            });

			entity.Bind<IInObjectTree>(
				c =>
			{
				_tree = c;
				_parent = _tree.TreeNode.Parent;
				_tree.TreeNode.OnParentChanged.Subscribe(onParentChanged);
				if (_parent != null) _parent.OnMatrixChanged.Subscribe(onSomethingChanged);
				onSomethingChanged(null);
			}, c =>
			{
				c.TreeNode.OnParentChanged.Unsubscribe(onParentChanged);
				if (c.TreeNode.Parent != null) c.TreeNode.Parent.OnMatrixChanged.Unsubscribe(onSomethingChanged);
				_tree = null;
				_parent = null;
				onSomethingChanged(null);
			});
        }

        public ModelMatrices GetModelMatrices() 
        { 
            return shouldRecalculate() ? recalculateMatrices() : _matrices; 
        }

        public IEvent<object> OnMatrixChanged { get; private set; }

        public static bool GetVirtualResolution(bool flattenLayerResolution, Size virtualResolution, IDrawableInfo drawable, 
                                         PointF? customResolutionFactor, out PointF resolutionFactor, out Size resolution)
        {
            //Priorities for virtual resolution: layer's resolution comes first, if not then the custom resolution (which is the text scaling resolution for text, otherwise null),
            //and if not use the virtual resolution.
            var renderLayer = drawable == null ? null : drawable.RenderLayer;
            var layerResolution = renderLayer == null ? null : renderLayer.IndependentResolution;
            if (layerResolution != null)
            {
                if (flattenLayerResolution)
                {
                    resolutionFactor = NoScaling;
                    resolution = virtualResolution;
                    return false;
                }
                resolution = layerResolution.Value;
                resolutionFactor = new PointF(resolution.Width / (float)virtualResolution.Width, resolution.Height / (float)virtualResolution.Height);
                return layerResolution.Value.Equals(virtualResolution);
            }
            else if (customResolutionFactor != null)
            {
                resolutionFactor = customResolutionFactor.Value;
                resolution = new Size((int)(virtualResolution.Width * customResolutionFactor.Value.X), 
                                      (int)(virtualResolution.Height * customResolutionFactor.Value.Y));
                return customResolutionFactor.Value.Equals(NoScaling);
            }
            else
            {
                resolutionFactor = NoScaling;
                resolution = virtualResolution;
                return true;
            }
        }

        private void onSomethingChanged(object args)
        {
            _isDirty = true;
        }

        private void onParentChanged(object args)
        {
            if (_parent != null) _parent.OnMatrixChanged.Unsubscribe(onSomethingChanged);
            _parent = _tree == null ? null : _tree.TreeNode.Parent;
            if (_parent != null) _parent.OnMatrixChanged.Subscribe(onSomethingChanged);
            onSomethingChanged(args);
        }

        private ISprite getSprite()
        {
            return _animation == null || _animation.Animation == null ? null : _animation.Animation.Sprite;
        }

        private void subscribeSprite(ISprite sprite)
        {
            changeSpriteSubscription(sprite, subscribeSpriteEvent);
        }

        private void unsubscribeSprite(ISprite sprite)
        {
            changeSpriteSubscription(sprite, unsubscribeSpriteEvent);
        }

        private void subscribeSpriteEvent(IEvent<object> ev)
        {
            ev.Subscribe(onSomethingChanged);
        }

        private void unsubscribeSpriteEvent(IEvent<object> ev)
        {
            ev.Unsubscribe(onSomethingChanged);
        }

        private void changeSpriteSubscription(ISprite sprite, Action<IEvent<object>> change)
        {
            if (sprite == null) return;
            change(sprite.OnLocationChanged);
            change(sprite.OnAngleChanged);
            change(sprite.OnScaleChanged);
            change(sprite.OnAnchorChanged);
        }

        private bool shouldRecalculate() 
        {
            PointF areaScaling = getAreaScaling();
            if (!_areaScaling.Equals(areaScaling)) 
            {
                _areaScaling = areaScaling;
                _isDirty = true;
            }
            var currentSprite = getSprite();
            if (currentSprite != _sprite)
            {
                unsubscribeSprite(_sprite);
                _sprite = currentSprite;
                subscribeSprite(_sprite);
                _isDirty = true;
            }
            var renderer = _image.CustomRenderer;
            if (renderer != null)
            {
                var customImageSize = renderer.CustomImageSize;
                if ((customImageSize == null && _customImageSize != null) || 
                    (customImageSize != null && _customImageSize == null) ||
                    !customImageSize.Value.Equals(_customImageSize.Value))
                {
                    _customImageSize = customImageSize;
                    _isDirty = true;
                }
                var customFactor = renderer.CustomImageResolutionFactor;
                if ((customFactor == null && _customResolutionFactor != null) ||
                    (customFactor != null && _customResolutionFactor == null) ||
                    !customFactor.Value.Equals(_customResolutionFactor.Value))
                {
                    _customResolutionFactor = customFactor;
                    _isDirty = true;
                }
            }
            return _isDirty;
        }

        private ModelMatrices recalculateMatrices()
        {
            recalculate();
            OnMatrixChanged.FireEvent(null);
            return _matrices;
        }

        private void recalculate()
        {
            PointF resolutionFactor;
            Size resolution;
            bool resolutionMatches = GetVirtualResolution(true, _virtualResolution, _drawable, _customResolutionFactor,
                                                   out resolutionFactor, out resolution);

            var renderMatrix = getMatrix(resolutionFactor);
            var hitTestMatrix = resolutionMatches ? renderMatrix : resolutionFactor.Equals(NoScaling) ? getMatrix(new PointF((float)_virtualResolution.Width/_drawable.RenderLayer.IndependentResolution.Value.Width,
                                                                                                                             (float)_virtualResolution.Height/_drawable.RenderLayer.IndependentResolution.Value.Height)) : getMatrix(NoScaling);
            _matrices.InObjResolutionMatrix = renderMatrix;
            _matrices.InVirtualResolutionMatrix = hitTestMatrix;
            _isDirty = false;
        }

        private Matrix4 getMatrix(PointF resolutionFactor) 
        {
            var sprite = _animation.Animation.Sprite;
            Matrix4 spriteMatrix = getModelMatrix(sprite, sprite, sprite, sprite, PointF.Empty, 
                                                  NoScaling, NoScaling, true);
            Matrix4 objMatrix = getModelMatrix(_scale, _rotate, _translate,
                                               _image, _crop == null ? PointF.Empty : new PointF(_crop.CropArea.X, _crop.CropArea.Y), _areaScaling, resolutionFactor, true);

            var modelMatrix = spriteMatrix * objMatrix;
            var parent = _tree == null ? null : _tree.TreeNode.Parent;
            while (parent != null)
            {
                //var parentMatrices = parent.GetModelMatrices();
                //Matrix4 parentMatrix = resolutionFactor.Equals(GLMatrixBuilder.NoScaling) ? parentMatrices.InVirtualResolutionMatrix : parentMatrices.InObjResolutionMatrix;
                Matrix4 parentMatrix = getModelMatrix(parent, parent, parent, parent, PointF.Empty, 
                    NoScaling, resolutionFactor, false);
                modelMatrix = modelMatrix * parentMatrix;
                parent = parent.TreeNode.Parent;
            }
            return modelMatrix;
        }

        private Matrix4 getModelMatrix(IScale scale, IRotate rotate, ITranslate translate, IHasImage image, PointF cropTranslate,
                                       PointF areaScaling, PointF resolutionTransform, bool useCustomImageSize)
        {
            if (scale == null) return Matrix4.Identity;
            float? customWidth = _customImageSize == null || !useCustomImageSize ? 
                _nullFloat : _customImageSize.Value.Width;
            float? customHeight = _customImageSize == null || !useCustomImageSize ? 
                _nullFloat : _customImageSize.Value.Height;
            float width = (customWidth ?? scale.Width) * resolutionTransform.X;
            float height = (customHeight ?? scale.Height) * resolutionTransform.Y;
            PointF anchorOffsets = getAnchorOffsets(image == null ? PointF.Empty : image.Anchor, 
                                                    width - cropTranslate.X, height - cropTranslate.Y);
            Matrix4 anchor = Matrix4.CreateTranslation(new Vector3(-anchorOffsets.X, -anchorOffsets.Y, 0f));
            Matrix4 scaleMat = Matrix4.CreateScale(new Vector3(scale.ScaleX * areaScaling.X,
                scale.ScaleY * areaScaling.Y, 1f));
            Matrix4 rotation = Matrix4.CreateRotationZ(rotate == null ? 0f : rotate.Angle);            
            float x = translate == null ? 0f : translate.X * resolutionTransform.X;
            float y = translate == null ? 0f : translate.Y * resolutionTransform.Y;
            Matrix4 transform = Matrix4.CreateTranslation(new Vector3(x, y, 0f));
            return anchor * scaleMat * rotation * transform;
        }

        private PointF getAnchorOffsets(PointF anchor, float width, float height)
        {
            float x = MathUtils.Lerp(0f, 0f, 1f, width, anchor.X);
            float y = MathUtils.Lerp(0f, 0f, 1f, height, anchor.Y);
            return new PointF(x, y);
        }

        private PointF getAreaScaling()
        {
            if (_room == null || (_drawable != null && _drawable.IgnoreScalingArea)) return NoScaling;
            foreach (IArea area in _room.Room.GetMatchingAreas(_translate.Location.XY, _entity.ID))
            {
                IScalingArea scaleArea = area.GetComponent<IScalingArea>();
                if (scaleArea == null || (!scaleArea.ScaleObjectsX && !scaleArea.ScaleObjectsY)) continue;
                float scale = scaleArea.GetScaling(scaleArea.Axis == ScalingAxis.X ? _translate.X : _translate.Y);
                return new PointF(scaleArea.ScaleObjectsX ? scale : 1f, scaleArea.ScaleObjectsY ? scale : 1f);
            }
            return NoScaling;
        }
    }
}
