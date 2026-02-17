using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene {
    public class ActionCard : CardBase
    {
        public string Title;
        public string Description;
        public string ImgPath;

        public override void PopulateCardUI(UICard toPopulate)
        {
            toPopulate.Title.SetText(Title);
            toPopulate.Description.SetText(Description);
            // TODO: img
            // toPopulate.Img.SetText(Title);
        }
    }
}