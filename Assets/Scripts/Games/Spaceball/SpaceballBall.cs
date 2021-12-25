using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RhythmHeavenMania.Util;

namespace RhythmHeavenMania.Games.Spaceball
{
    public class SpaceballBall : MonoBehaviour
    {
        public float startBeat;
        private Animator anim;
        private int lastState;
        private bool inList = false;

        private Minigame.Eligible e = new Minigame.Eligible();

        public GameObject Holder;

        private void Start()
        {
            anim = GetComponent<Animator>();

            e.gameObject = this.gameObject;
        }

        private void Update()
        {
            float normalizedBeatAnim = Conductor.instance.GetLoopPositionFromBeat(startBeat, 1.25f);
            anim.Play("BallLow", -1, normalizedBeatAnim);

            float normalizedBeat = Conductor.instance.GetLoopPositionFromBeat(startBeat, 1f);

            if (normalizedBeat > Minigame.EarlyTime() && normalizedBeat < Minigame.PerfectTime() && lastState == 0)
            {
                MakeEligible(true, false, false);
                lastState++;
            }
            // Perfect State
            else if (normalizedBeat > Minigame.PerfectTime() && normalizedBeat < Minigame.LateTime() && lastState == 1)
            {
                MakeEligible(false, true, false);
                lastState++;
            }
            // Late State
            else if (normalizedBeat > Minigame.LateTime() && normalizedBeat < Minigame.EndTime() && lastState == 2)
            {
                MakeEligible(false, false, true);
                lastState++;
            }
            else if (normalizedBeat < Minigame.EarlyTime() || normalizedBeat > Minigame.EndTime())
            {
                MakeInEligible();
            }

            if (normalizedBeat > 1.25f && lastState == 3)
            {
                lastState++;
                Jukebox.PlayOneShotGame("spaceball/fall");
                Instantiate(Spaceball.instance.Dust, Spaceball.instance.Dust.transform.parent).SetActive(true);
                Destroy(this.gameObject);
            }
        }

        public void MakeEligible(bool early, bool perfect, bool late)
        {
            // print($"{early}, {perfect}, {late}");

            if (!inList)
            {
                e.early = early;
                e.perfect = perfect;
                e.late = late;

                SpaceballPlayer.instance.EligibleHits.Add(e);
                inList = true;
            }
            else
            {
                Minigame.Eligible es = SpaceballPlayer.instance.EligibleHits[SpaceballPlayer.instance.EligibleHits.IndexOf(e)];
                es.early = early;
                es.perfect = perfect;
                es.late = late;
            }
        }

        public void MakeInEligible()
        {
            if (!inList) return;

            SpaceballPlayer.instance.EligibleHits.Remove(e);
            inList = false;
        }
    }

}