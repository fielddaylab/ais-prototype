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

        public SpeciesCluster CreateSpeciesCluster(Transform parentTransform)
        {
            var newCluster = Instantiate(SpeciesClusterPrefab, parentTransform).GetComponent<SpeciesCluster>();

            return newCluster;
        }
    }
}