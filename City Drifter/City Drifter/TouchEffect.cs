using System;
using Xamarin.Forms;

namespace TouchTracking
{
    public class TouchEffect : RoutingEffect
    {
        public event TouchActionEventHandler TouchAction;

        public TouchEffect() : base("XamarinDocs.TouchEffect")
        {
        }

        public bool Capture { set; get; }

        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    //if (!isBeingDragged)
                    //{
                    //    isBeingDragged = true;
                    //    touchId = args.Id;
                    //pressPoint = args.Location;                        
                    //}
                    Console.WriteLine("TouchEffect Pressed ACTION");
                    break;

                case TouchActionType.Moved:
                    //if (isBeingDragged && touchId == args.Id)
                    //{
                    //    TranslationX += args.Location.X - pressPoint.X;
                    //    TranslationY += args.Location.Y - pressPoint.Y;
                    //}
                    Console.WriteLine("TouchEffect Moved ACTION");
                    break;

                case TouchActionType.Released:
                    //if (isBeingDragged && touchId == args.Id)
                    //{
                    //    isBeingDragged = false;
                    // }                    
                    Console.WriteLine("TouchEffect Released ACTION");
                    break;
            }
            TouchAction?.Invoke(element, args);
        }
    }
}
