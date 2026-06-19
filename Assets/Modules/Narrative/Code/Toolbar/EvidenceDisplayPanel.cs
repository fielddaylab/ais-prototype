using System;
using FieldDay;
using FieldDay.UI;
using FieldDay.UI.Widgets;
using TMPro;
using UnityEngine;

namespace AIS.Narrative {
    public sealed class EvidenceDisplayPanel : SharedPanel, IParameterizedGuiPanel<PlayerInventory> {
        public EvidenceDisplayWidget[] Widgets;
        public StatDisplayWidget[] Stats;

        protected override void Awake() {
            base.Awake();
            Stats = GetComponentsInChildren<StatDisplayWidget>();
            Hide();
        }

        public void Populate(in PlayerInventory parms) {
            int widgetIndex = 0;
            foreach (var evidenceId in parms.EvidenceChips) {
                EvidenceCard data = Find.NamedAsset<EvidenceCard>(evidenceId);

                EvidenceDisplayWidget widget = Widgets[widgetIndex++];
                widget.gameObject.SetActive(true);
                widget.Illustration = data.Illustration;
                widget.Type.sprite = data.isActionable ? actionableIcon : modelIcon;
                if (data.isActionable)
                {
                    widget.Type.sprite = actionableIcon;
                    widget.ActionSlot.gameObject.SetActive(true);
                    
                    widget.ActionContents.GetComponentInChildren<TMP_Text>().SetText(data.ActivateLocation.ToString());
                }
                else
                {
                    widget.Type.sprite = modelIcon;
                    widget.ActionSlot.gameObject.SetActive(false);
                }

                widget.Content.SetText(data.Label);
            }

            for(; widgetIndex < Widgets.Length; widgetIndex++) {
                Widgets[widgetIndex].gameObject.SetActive(false);
            }
        }

        public void PopulateStats(in PlayerStatBlock parms)
        {
            for (int i = 0; i < Stats.Length; i++)
            {
                StatDisplayWidget widget = Stats[i];
                widget.StatValueCounter.SetValue(parms[widget.StatId], GuiWidgetUpdateFlags.Force | GuiWidgetUpdateFlags.NoAnimation);
            }
        }

        public override void Show() {
            base.Show();
            Game.Gui.PushPriority(m_InputLayer);
        }

        public override void Hide() {
            Game.Gui.PopPriority(m_InputLayer);
            base.Hide();
        }
    }
}