namespace HNS.CozyWinterJam2022.Behaviours
{
    using HNS.CozyWinterJam2022.Models;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("CWJ2022/Gameworld")]

    public class GameworldBehaviour : MonoBehaviour
    {
        #region Members

        public int MapWidth;
        public int MapHeight;

        public Dictionary<Tuple<float, float>, BuildingBehaviour> Buildings { get; set; }

        public float[] Production { get; set; }

        public float[] Inventory { get; set; }

        public int[,] WorldMap { get; set; }

        #endregion

        #region Event Handlers

        private void Building_BuildComplete(object sender, EventArgs e)
        {
            var building = (BuildingBehaviour)sender;

            var cellX = (int)Mathf
                .Round(building.transform.position.x + 25);

            var cellY = (int)Mathf
                .Round(building.transform.position.z + 25);

            for (int cy = cellY - 1; cy <= cellY + 1; cy++)
            {
                for (int cx = cellX - 1; cx <= cellX + 1; cx++)
                {
                    if (cx < 0 || cx >= 50 || cy < 0 || cy >= 50)
                    {
                        continue;
                    }

                    var mapValue = WorldMap[cy, cx];
                    for (int i = 0; i < building.ResourcesProducedCategories.Length; i++)
                    {
                        var category = building.ResourcesProducedCategories[i];

                        var categoryIndex = (int)category;
                        if (mapValue != categoryIndex)
                        {
                            continue;
                        }

                        var amount = building.ResourcesProducedAmounts[i];
                        Production[categoryIndex] += amount;
                    }
                }
            }

            if (Buildings.Count > 1)
            {
                var roads = FindObjectOfType<RoadsBehaviour>();

                var firstVertex = new Vector3(building.transform.position.x, -0.5f, building.transform.position.z);
                var secondVertex = new Vector3(building.transform.position.x, -0.5f, building.transform.position.z);

                float minDistance = float.MaxValue;
                foreach (var otherBuilding in Buildings.Keys)
                {
                    var dx = otherBuilding.Item1 - firstVertex.x;
                    var dy = otherBuilding.Item2 - firstVertex.z;

                    var d = (dx * dx) + (dy * dy);
                    if (d > 0 && d < minDistance)
                    {
                        minDistance = d;
                        secondVertex.x = otherBuilding.Item1;
                        secondVertex.z = otherBuilding.Item2;
                    }
                }

                roads
                    .AddRoads(firstVertex, secondVertex);
            }
        }

        #endregion

        #region Methods

        public bool BuildingExists(float x, float z)
        {
            var key = new Tuple<float, float>(x, z);

            return Buildings
                .ContainsKey(key);
        }

        public void AddBuilding(float x, float z, BuildingBehaviour building)
        {
            var key = new Tuple<float, float>(x, z);
            Buildings[key] = building;
            building.BuildComplete += Building_BuildComplete;
        }

        protected void CreateWorldMap()
        {
            WorldMap = new int[MapHeight, MapWidth];

            for (int z = 0; z < MapHeight; z++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    WorldMap[z, x] = -1;

                    if (UnityEngine.Random.Range(0, 100) > 70)
                    {
                        var resourceIndex = UnityEngine.Random.Range(0, Production.Length);

                        var prefab = Resources
                            .Load<ResourceBehaviour>("Prefabs/Wood");

                        var resourceObject = Instantiate(prefab);
                        resourceObject.transform.position = new Vector3(x - 25, 0, z - 25);
                        resourceObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

                        WorldMap[z, x] = resourceIndex;
                    }
                }
            }
        }

        protected void Update()
        {
            for (int i = 0; i < Production.Length; i++)
            {
                Inventory[i] += Production[i] * Time.deltaTime;
            }
        }

        protected void Awake()
        {
            Buildings = new Dictionary<Tuple<float, float>, BuildingBehaviour>();

            var resources = Enum
                .GetValues(typeof(ProduceableResourceCategory));

            Production = new float[resources.Length];
            Inventory = new float[resources.Length];

            CreateWorldMap();            
        }
       
        #endregion
    }
}