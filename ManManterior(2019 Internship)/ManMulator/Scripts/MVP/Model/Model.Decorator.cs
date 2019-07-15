using System;
using System.Collections.Generic;
using UnityEngine;

partial class Model
{

    /// <summary>
    /// DecoratePresenter에서 사용할 바닥재, 벽지, 가구의 재질, 프리팹, UI 스프라이트 등을 담은 클래스.
    /// </summary>
    [Serializable]
    public class Decorator
    {
        #region Fields
        private Material[] floorMaterials;
        private Material[] wallMaterials;

        private String[] wallScripts;

        private Sprite[] floorSprites;
        private Sprite[] wallSprites;
        
        GameObject customizeFurniture;
        Sprite customizeFurnitureSprite;

        private GameObject[] furnitureForCeiling;
        private GameObject[] furnitureForFloor;
        private GameObject[] furnitureForWall;

        private Sprite[] furnitureForCeilingSprites;
        private Sprite[] furnitureForFloorSprites;
        private Sprite[] furnitureForWallSprites;
        
        public GameObject ItemIconPrefab;
        public GameObject EstimateIconPrefab;
       

        public Material[] FloorMaterials { get => floorMaterials; set => floorMaterials = value; }
        public Material[] WallMaterials { get => wallMaterials; set => wallMaterials = value; }
        public Sprite[] FloorSprites { get => floorSprites; set => floorSprites = value; }
        public Sprite[] WallSprites { get => wallSprites; set => wallSprites = value; }
        public string[] WallScripts { get => wallScripts; set => wallScripts = value; }
        public GameObject[] FurnitureForCeiling { get => furnitureForCeiling; set => furnitureForCeiling = value; }
        public GameObject[] FurnitureForFloor { get => furnitureForFloor; set => furnitureForFloor = value; }
        public Sprite[] FurnitureForCeilingSprites { get => furnitureForCeilingSprites; set => furnitureForCeilingSprites = value; }
        public Sprite[] FurnitureForFloorSprites { get => furnitureForFloorSprites; set => furnitureForFloorSprites = value; }
        public Sprite CustomizeFurnitureSprite { get => customizeFurnitureSprite; set => customizeFurnitureSprite = value; }
        public GameObject[] FurnitureForWall { get => furnitureForWall; set => furnitureForWall = value; }
        public Sprite[] FurnitureForWallSprites { get => furnitureForWallSprites; set => furnitureForWallSprites = value; }
        public GameObject CustomizeFurniture { get => customizeFurniture; set => customizeFurniture = value; }
        #endregion

        #region Methods

        #endregion
    }

}
