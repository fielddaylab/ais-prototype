using AIS.Intervene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    public class InvasionModelPrefabs : MonoBehaviour
    {
        public static InvasionModelPrefabs Instance;

        public GameObject EcosystemPrefab;
        public GameObject PathwayPrefab;
        public GameObject SpeciesClusterPrefab;

        public GameObject TransformPrefab;

        private void Awake()
        {
            Instance = this;
        }

        public Cluster CreateCluster(Transform parentTransform, ActionTarget targetType)
        {
            Cluster newCluster = Instantiate(SpeciesClusterPrefab, parentTransform).GetComponent<Cluster>();

            switch (targetType) {
                case ActionTarget.Nest:
                    var nest = newCluster.gameObject.AddComponent<Nest>();
                    nest.SpawnSpeciesId = InvasionModel.Instance.m_CurrModelSetupData.DefaultInvasive.SpeciesId;
                    nest.SpawnAmt = 1;
                    nest.SpawnTravelType = InvasionModel.Instance.m_CurrModelSetupData.DefaultInvasive.StartingTravelType;
                    nest.SpawnTargetType = InvasionModel.Instance.m_CurrModelSetupData.DefaultInvasive.StartingTargetType;
                    nest.TriggerOdds = 1 / 6f;
                    break;
                case ActionTarget.Trap:
                    var trap = newCluster.gameObject.AddComponent<Trap>();
                    trap.TrapSpeciesId = InvasionModel.Instance.m_CurrModelSetupData.DefaultInvasive.SpeciesId;
                    trap.TrapAmt = 1;
                    trap.TriggerOdds = 1 / 6f;
                    break;
                default:
                    break;
            }

            return newCluster;
        }
    }
}