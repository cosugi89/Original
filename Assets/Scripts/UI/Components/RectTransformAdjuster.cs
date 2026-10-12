using UnityEngine;
using UnityEngine.UI;
 
namespace Assets.Scripts.UI.Components
{
    /*
    /// <summary>
    /// RectTransformの自動調整のジャンル非依存ロジックを担う基底クラス。
    /// このクラスを直接 prefab にアタッチせず、ジャンルごとに具象クラスを作って prefab に載せる。
    /// </summary>
    [ExecuteAlways]
    public abstract class RectTransformAdjusterBase : MonoBehaviour
    {
     private enum AdjustDirection
     {
         Vertical,
         Horizontal
     }

     #region SerializeField
     [SerializeField] private AdjustDirection _adjustDirection;
     [SerializeField] private RectTransform _targetRectTransform;
     [SerializeField] private RectTransform[] _layoutElementRectTransforms;
     [SerializeField] private float _minWidth;
     [SerializeField] private float _minHeight;
     [SerializeField] private float _paddingTop;
     [SerializeField] private float _paddingBottom;
     [SerializeField] private float _paddingLeft;
     [SerializeField] private float _paddingRight;
     [SerializeField] private float _space;
     #endregion SerializeField

     public void AdjustSize()
     {
         switch (_adjustDirection)
         {
             case AdjustDirection.Horizontal:
                 AdjustWidth();
                 break;

             case AdjustDirection.Vertical:
                 AdjustHeight();
                 break;
         }
     }

     private void AdjustWidth()
     {
         var width = _paddingLeft + _paddingRight;

         var itemCount = 0;
         foreach (var layoutElementRectTransform in _layoutElementRectTransforms)
         {
    #if UNITY_EDITOR
             if (!Application.isPlaying && (layoutElementRectTransform == null))
             {
                 continue;
             }
    #endif

             if (layoutElementRectTransform.gameObject.activeSelf)
             {
                 width += layoutElementRectTransform.sizeDelta.x * layoutElementRectTransform.localScale.x;
                 if (itemCount > 0)
                 {
                     width += _space;
                 }

                 itemCount++;
             }
         }

         width = Mathf.Max(_minWidth, width);
         SetSize(new Vector2(width, _targetRectTransform.sizeDelta.y));
     }

     private void AdjustHeight()
     {
         var height = _paddingTop + _paddingBottom;
         var itemCount = 0;
         foreach (var layoutElementRectTransform in _layoutElementRectTransforms)
         {
    #if UNITY_EDITOR
             if (!Application.isPlaying && layoutElementRectTransform == null)
             {
                 continue;
             }
    #endif

             if (layoutElementRectTransform.gameObject.activeSelf)
             {
                 height += layoutElementRectTransform.sizeDelta.y * layoutElementRectTransform.localScale.y;

                 if (itemCount > 0)
                 {
                     height += _space;
                 }

                 itemCount++;
             }
         }

         height = Mathf.Max(_minHeight, height);
         SetSize(new Vector2(_targetRectTransform.sizeDelta.x, height));
     }

     private void SetSize(Vector2 size)
     {
         if (size.Equals(_targetRectTransform.sizeDelta))
         {
             return;
         }

         _targetRectTransform.sizeDelta = size;
         LayoutRebuilder.MarkLayoutForRebuild(_targetRectTransform);
     }

     public void PackOnMiddleTopLayoutElement()
     {
         // 下にずれていくため、paddingの符号を変える
         var offset = _paddingTop * -1;
         foreach (var layoutElementRectTransform in _layoutElementRectTransforms)
         {
    #if UNITY_EDITOR
             if (!Application.isPlaying && layoutElementRectTransform == null)
             {
                 continue;
             }
    #endif

             if (layoutElementRectTransform.gameObject.activeSelf)
             {
                 var beforeAnchorMax = layoutElementRectTransform.anchorMax;
                 var beforeAnchorMin = layoutElementRectTransform.anchorMin;
                 var beforePivot = layoutElementRectTransform.pivot;

                 layoutElementRectTransform.SetAnchorTopWithKeepingPosition();
                 layoutElementRectTransform.SetPivotWithKeepingPosition(0.5f, 1.0f);
                 layoutElementRectTransform.SetAnchoredPositionY(offset);

                 layoutElementRectTransform.SetAnchorWithKeepingPosition(beforeAnchorMin, beforeAnchorMax);
                 layoutElementRectTransform.SetPivotWithKeepingPosition(beforePivot);

                 var height = layoutElementRectTransform.rect.height * layoutElementRectTransform.localScale.y;
                 offset -= height + _space;
             }
         }
     }

     public void PackOnLeftCenterLayoutElement()
     {
         var offset = _paddingLeft;
         foreach (var layoutElementRectTransform in _layoutElementRectTransforms)
         {
    #if UNITY_EDITOR
             if (!Application.isPlaying && layoutElementRectTransform == null)
             {
                 continue;
             }
    #endif

             if (layoutElementRectTransform.gameObject.activeSelf)
             {
                 var beforeAnchorMax = layoutElementRectTransform.anchorMax;
                 var beforeAnchorMin = layoutElementRectTransform.anchorMin;
                 var beforePivot = layoutElementRectTransform.pivot;

                 layoutElementRectTransform.SetAnchorLeftWithKeepingPosition();
                 layoutElementRectTransform.SetPivotWithKeepingPosition(0.0f, 0.5f);
                 layoutElementRectTransform.SetAnchoredPositionX(offset);

                 layoutElementRectTransform.SetAnchorWithKeepingPosition(beforeAnchorMin, beforeAnchorMax);
                 layoutElementRectTransform.SetPivotWithKeepingPosition(beforePivot);

                 var width = layoutElementRectTransform.rect.width * layoutElementRectTransform.localScale.x;
                 offset += width + _space;
             }
         }
     }

     public void SetSpace(float space)
     {
         _space = space;
     }

    #if UNITY_EDITOR
     private void Update()
     {
         if (!Application.isPlaying)
         {
             if (_layoutElementRectTransforms == null || _targetRectTransform == null)
             {
                 return;
             }

             AdjustSize();

             switch (_adjustDirection)
             {
                 case AdjustDirection.Horizontal:
                     PackOnLeftCenterLayoutElement();
                     break;

                 case AdjustDirection.Vertical:
                     PackOnMiddleTopLayoutElement();
                     break;
             }
         }
     }

     public void MultiplyAdjustValue(float scaleFactorX, float scaleFactorY)
     {
         _minWidth *= scaleFactorX;
         _paddingLeft *= scaleFactorX;
         _paddingRight *= scaleFactorX;

         _minHeight *= scaleFactorY;
         _paddingTop *= scaleFactorY;
         _paddingBottom *= scaleFactorY;

         _space *= (_adjustDirection == AdjustDirection.Vertical) ? scaleFactorY : scaleFactorX;
     }
    #endif
    }
    */
}