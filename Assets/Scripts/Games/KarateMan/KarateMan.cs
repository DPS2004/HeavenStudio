using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RhythmHeavenMania.Util;

namespace RhythmHeavenMania.Games.KarateMan
{
    public class KarateMan : Minigame
    {
        public GameObject Pot, Bomb;
        public KarateJoe KarateJoe;

        public List<Minigame.Eligible> EligibleCombos = new List<Minigame.Eligible>();

        public static KarateMan instance { get; set; }

        public Sprite[] ObjectSprites;
        public Sprite[] BarrelSprites;

        public List<BGSpriteC> BGSprites;
        public SpriteRenderer BGSprite;

        private bool bgEnabled;
        private float newBeat, newBeatBop;

        private float bopLength;
        private float bopBeat;

        private float bgBeat;

        public GameObject comboRef;

        [System.Serializable]
        public class BGSpriteC
        {
            public List<Sprite> Sprites;
        }

        private void Awake()
        {
            instance = this;
        }

        public void Combo(float beat)
        {
            comboRef.GetComponent<Animator>().enabled = true;
            comboRef.GetComponent<Animator>().Play("comboRef");
            Jukebox.PlayOneShotGame("karateman/barrelOutCombos");

            Shoot(beat, 0, true, "PotCombo1", 0, new Vector2(-0.94f, -2.904f));
            Shoot(beat + 0.25f, 0, true, "PotCombo2", 1, new Vector2(-0.94f, -2.904f));
            Shoot(beat + 0.5f, 0, true, "PotCombo3", 2, new Vector2(-0.776f, -3.162f));
            Shoot(beat + 0.75f, 0, true, "PotCombo4", 3, new Vector2(1.453f, -3.162f));
            Shoot(beat + 1f, 0, true, "PotCombo5", 4, new Vector2(0.124f, -3.123f));
            Shoot(beat + 1.5f, 4, true, "PotCombo6", 5, new Vector2(-1.333f, -2.995f));

            GameObject cs = new GameObject(); cs.AddComponent<ComboSound>().startBeat = beat;
            cs.transform.parent = this.transform.parent;
        }

        public void Shoot(float beat, int type, bool combo = false, string throwAnim = "", int comboIndex = 0, Vector2 endShadowPos = new Vector2())
        {
            GameObject pot = Instantiate(Pot);
            pot.transform.parent = Pot.transform.parent;


            Pot p = pot.GetComponent<Pot>();

            pot.SetActive(true);
            p.startBeat = beat;
            p.createBeat = beat;
            p.isThrown = true;
            p.type = type;
            p.Sprite.GetComponent<SpriteRenderer>().sprite = ObjectSprites[type];

            if (combo)
            {
                p.comboIndex = comboIndex;
                p.throwAnim = throwAnim;
                p.combo = true;
                KarateJoe.currentComboPots.Add(p);
                p.endShadowThrowPos = endShadowPos;
            }
            else
            {
                p.throwAnim = "PotThrow";

                string outSnd = "";
                switch (type)
                {
                    case 0:
                        if (Starpelly.Mathp.GetDecimalFromFloat(beat) == 0f)
                            outSnd = "karateman/objectOut";
                        else
                            outSnd = "karateman/offbeatObjectOut";
                        p.hitSnd = "karateman/potHit";
                        break;
                    case 1:
                        outSnd = "karateman/lightbulbOut";
                        p.hitSnd = "karateman/lightbulbHit";
                        break;
                    case 2:
                        outSnd = "karateman/objectOut";
                        p.hitSnd = "karateman/rockHit";
                        break;
                    case 3:
                        outSnd = "karateman/objectOut";
                        p.hitSnd = "karateman/soccerHit";
                        break;
                    case 4:
                        p.kick = true;
                        outSnd = "karateman/barrelOutKicks";
                        p.hitSnd = "karateman/barrelBreak";

                        GameObject pks = new GameObject(); pks.AddComponent<PunchKickSound>().startBeat = beat;
                        pks.transform.parent = this.transform.parent;
                        break;
                }

                p.endShadowThrowPos = new Vector2(-1.036f, -2.822f);

                Jukebox.PlayOneShotGame(outSnd);
            }
        }

        private void Update()
        {
            if (Conductor.instance.ReportBeat(ref newBeat))
            {
                if (bgEnabled)
                {
                    if (bgBeat % 2 == 0)
                    {
                        BGSprite.sprite = BGSprites[0].Sprites[1];
                    }
                    else
                    {
                        BGSprite.sprite = BGSprites[0].Sprites[2];
                    }
                    bgBeat++;
                }
            }

            if (Conductor.instance.ReportBeat(ref newBeatBop, bopBeat % 1))
            {
                if (Conductor.instance.songPositionInBeats >= bopBeat && Conductor.instance.songPositionInBeats < bopBeat + bopLength)
                {
                    float compare = KarateJoe.anim.GetCurrentAnimatorStateInfo(0).speed;
                    if (KarateJoe.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= compare && !KarateJoe.anim.IsInTransition(0))
                        KarateJoe.anim.Play("Bop", 0, 0);
                }
            }
        }

        public void BGFXOn()
        {
            bgEnabled = true;
        }

        public void BGFXOff()
        {
            bgEnabled = false;
            BGSprite.sprite = BGSprites[0].Sprites[0];
        }

        public void Bop(float beat, float length)
        {
            bopLength = length;
            bopBeat = beat;
        }

        public void CreateBomb(Transform parent, Vector2 scale, ref GameObject shadow)
        {
            GameObject bomb = Instantiate(Bomb, parent);
            bomb.SetActive(true);
            bomb.transform.localScale = scale;
            shadow.transform.parent = bomb.transform;
            shadow.transform.SetAsLastSibling();
            bomb.GetComponent<Bomb>().shadow = shadow;
        }
    }
}