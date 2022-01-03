using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmHeavenMania
{
    public class Initializer : MonoBehaviour
    {
        public TextAsset level;
        public AudioClip music;

        private void Start()
        {
            GameObject Cameras = Instantiate(Resources.Load<GameObject>("Prefabs/Cameras")); Cameras.name = "Cameras";
            GameObject MainCamera = Cameras.transform.GetChild(0).gameObject;
            GameObject CursorCamera = Cameras.transform.GetChild(1).gameObject;

            GameObject Cursor = Instantiate(Resources.Load<GameObject>("Prefabs/Cursor"));
            Cursor.name = "Cursor";

            GameObject Games = new GameObject();
            Games.name = "Games";

            GameObject GameManager = new GameObject();
            GameManager.name = "GameManager";
            GameManager gameManager = GameManager.AddComponent<GameManager>();

            gameManager.txt = level;
            gameManager.GamesHolder = Games;
            gameManager.CircleCursor = Cursor.transform.GetChild(0).GetComponent<CircleCursor>();
            gameManager.GameCamera = MainCamera.GetComponent<Camera>();
            gameManager.CursorCam = CursorCamera.GetComponent<Camera>();

            GameObject Profiler = Instantiate(Resources.Load<GameObject>("Prefabs/GameProfiler"));
            Profiler.name = "GameProfiler";

            GameObject Conductor = new GameObject();
            Conductor.name = "Conductor";
            Conductor.AddComponent<AudioSource>().clip = music;
            Conductor.AddComponent<Conductor>();
            Conductor.AddComponent<AudioDspTimeKeeper>();
        }
    }
}