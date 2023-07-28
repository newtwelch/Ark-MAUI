using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Ark.Models
{
    public partial class DeviceOrientationPartialClass
    {
        private static readonly IReadOnlyDictionary<DisplayOrientation, UIInterfaceOrientation> _iosDisplayOrientationMap =
            new Dictionary<DisplayOrientation, UIInterfaceOrientation>
            {
                [DisplayOrientation.Landscape] = UIInterfaceOrientation.LandscapeLeft,
                [DisplayOrientation.Portrait] = UIInterfaceOrientation.Portrait,
            };

        public partial void SetDeviceOrientation(DisplayOrientation displayOrientation)
        {
            if (_iosDisplayOrientationMap.TryGetValue(displayOrientation, out var iosOrientation))
                UIApplication.SharedApplication.SetStatusBarOrientation(iosOrientation, true);
        }
    }
}
