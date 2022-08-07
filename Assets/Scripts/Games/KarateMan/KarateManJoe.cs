using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_KarateMan
{
    public class KarateManJoe : MonoBehaviour
    {
        public Animator anim;
        public Animator FaceAnim;
        public GameEvent bop = new GameEvent();
        public SpriteRenderer[] Shadows;

        float lastPunchTime = Single.MinValue;
        float lastComboMissTime = Single.MinValue;
        float lastUpperCutTime = Single.MinValue;
        public bool inCombo = false;
        public bool lockedInCombo = false;
        int inComboId = -1;
        int shouldComboId = -1;
        public void SetComboId(int id) { inComboId = id; }
        public void SetShouldComboId(int id) { shouldComboId = id; }
        public int GetComboId() { return inComboId; }
        public int GetShouldComboId() { return shouldComboId; }

        public bool wantKick = false;
        public bool inKick = false;
        float lastChargeTime = Single.MinValue;
        float unPrepareTime = Single.MinValue;
        bool canEmote = false;
        public int wantFace = 0;

        bool inSpecial { get { return inCombo || lockedInCombo || Conductor.instance.GetPositionFromBeat(lastChargeTime, 2.75f) <= 0.25f; } }

        private void Awake()
        {

        }

        private void Update()
        {
            var cond = Conductor.instance;

            if (unPrepareTime != Single.MinValue && cond.songPositionInBeats >= unPrepareTime)
            {
                unPrepareTime = Single.MinValue;
                anim.speed = 1f;
                anim.Play("Beat", -1, 0);
            }

            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1, false) && cond.songPositionInBeats > bop.startBeat && cond.songPositionInBeats >= unPrepareTime && !inCombo)
            {
                anim.speed = 1f;
                anim.Play("Beat", -1, 0);
                lastChargeTime = Single.MinValue;
            }

            if (inCombo && shouldComboId == -2)
            {
                float missProg = cond.GetPositionFromBeat(lastComboMissTime, 3f);
                if (missProg >= 0f && missProg < 1f)
                {
                    anim.DoScaledAnimation("LowKickMiss", lastComboMissTime, 3f);
                    bop.startBeat = lastComboMissTime + 3f;
                }
                else if (missProg >= 1f)
                {
                    anim.speed = 1f;
                    bop.startBeat = lastComboMissTime + 3f;
                    lastComboMissTime = Single.MinValue;
                    inCombo = false;
                    inComboId = -1;
                    shouldComboId = -1;
                }
            }

            if (inKick)
            {
                float chargeProg = cond.GetPositionFromBeat(lastChargeTime, 2.75f);
                if (chargeProg >= 0f && chargeProg < 1f)
                {
                    anim.DoScaledAnimation("ManCharge", lastChargeTime, 2.75f);
                    bop.startBeat = lastChargeTime + 1.75f;
                }
                else if (chargeProg >= 1f)
                {
                    anim.speed = 1f;
                    bop.startBeat = lastChargeTime + 1.75f;
                    lastChargeTime = Single.MinValue;
                    inKick = false;
                }
            }

            if (PlayerInput.Pressed(true) && !inSpecial)
            {
                if (!KarateMan.instance.IsExpectingInputNow())
                {
                    Punch(1);
                    Jukebox.PlayOneShotGame("karateman/swingNoHit", forcePlay: true);
                }
            }
            else if (PlayerInput.AltPressed() && !inSpecial)
            {
                if (!KarateMan.instance.IsExpectingInputNow())
                {
                    //start a forced-fail combo sequence
                    ForceFailCombo(cond.songPositionInBeats);
                }
            }
            else if (PlayerInput.AltPressedUp())
            {
                if (!KarateMan.instance.IsExpectingInputNow())
                {
                    if (inComboId != -1 && !lockedInCombo)
                    {
                        inComboId = -1;
                    }
                }
            }

            if ((!GameManager.instance.autoplay) && (PlayerInput.PressedUp(true) && !PlayerInput.Pressing(true)))
            {
                if (wantKick)
                {
                    //stopped holding, don't charge
                    wantKick = false;
                }
                else if (inKick && cond.GetPositionFromBeat(lastChargeTime, 2.75f) <= 0.5f && !KarateMan.instance.IsExpectingInputNow())
                {
                    Kick(cond.songPositionInBeats);
                    Jukebox.PlayOneShotGame("karateman/swingKick", forcePlay: true);
                }
            }

            UpdateShadowColour();

            if (canEmote && wantFace >= 0)
            {
                SetFaceExpressionForced(wantFace);
                if (wantFace == (int) KarateMan.KarateManFaces.Surprise) wantFace = -1;
            }
        }

        public bool Punch(int forceHand = 0)
        {
            if (GameManager.instance.currentGame != "karateman") return false;
            var cond = Conductor.instance;
            bool straight = false;

            anim.speed = 1f;
            unPrepareTime = Single.MinValue;
            lastChargeTime = Single.MinValue;
            inKick = false;

            switch (forceHand)
            {
                case 0:
                    if (cond.songPositionInBeats - lastPunchTime < 0.25f + (Minigame.LateTime() - 1f))
                    {
                        lastPunchTime = Single.MinValue;
                        anim.DoScaledAnimationAsync("Straight", 0.5f);
                        straight = true;
                    }
                    else
                    {
                        lastPunchTime = cond.songPositionInBeats;
                        anim.DoScaledAnimationAsync("Jab", 0.5f);
                    }
                    break;
                case 1:
                    anim.DoScaledAnimationAsync("Jab", 0.5f);
                    break;
                case 2:
                    anim.DoScaledAnimationAsync("Straight", 0.5f);
                    straight = true;
                    break;
            }
            bop.startBeat = cond.songPositionInBeats + 0.5f;
            return straight;    //returns what hand was used to punch the object
        }

        public void ComboSequence(int seq)
        {
            if (GameManager.instance.currentGame != "karateman") return;
            var cond = Conductor.instance;
            bop.startBeat = cond.songPositionInBeats + 1f;
            unPrepareTime = Single.MinValue;
            switch (seq)
            {
                case 0:
                    anim.Play("LowJab", -1, 0);
                    break;
                case 1:
                    anim.Play("LowKick", -1, 0);
                    break;
                case 2:
                    anim.DoScaledAnimationAsync("BackHand", 0.5f);
                    break;
                case 3:
                    anim.DoScaledAnimationAsync("UpperCut", 0.5f);
                    lockedInCombo = false;
                    break;
                case 4:
                    anim.Play("ToReady", -1, 0);
                    bop.startBeat = cond.songPositionInBeats + 0.5f;
                    lockedInCombo = false;
                    break;
                default:
                    break;
            }
        }

        public void ComboMiss(float beat)
        {
            var cond = Conductor.instance;
            lastComboMissTime = beat;
            bop.startBeat = beat + 3f;
            unPrepareTime = Single.MinValue;
            anim.DoNormalizedAnimation("LowKickMiss");
        }

        public void ForceFailCombo(float beat)
        {
            if (inCombo) return;
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Punch(1); inCombo = true; inComboId = -1; shouldComboId = -1;}),
                new BeatAction.Action(beat + 0.25f, delegate { Punch(2); }),
                new BeatAction.Action(beat + 0.5f, delegate { ComboSequence(0); }),
                new BeatAction.Action(beat + 0.75f, delegate { shouldComboId = -2; ComboMiss(beat + 0.75f); }),
            });

            MultiSound.Play(new MultiSound.Sound[] 
            {
                new MultiSound.Sound("karateman/swingNoHit", beat), 
                new MultiSound.Sound("karateman/swingNoHit_Alt", beat + 0.25f), 
                new MultiSound.Sound("karateman/swingNoHit_Alt", beat + 0.5f), 
                new MultiSound.Sound("karateman/comboMiss", beat + 0.75f),  
            }, forcePlay: true);
        }

        public void StartKickCharge(float beat)
        {
            wantKick = true;
            unPrepareTime = Single.MinValue;
            BeatAction.New(gameObject, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { 
                    if (wantKick)
                    {
                        wantKick = false;
                        inKick = true;
                        lastChargeTime = beat;
                        bop.startBeat = beat + 1.75f;
                    }
                })
            });
        }

        public void Kick(float beat)
        {
            if (!inKick) return;
            //play the kick animation and reset stance
            anim.speed = 1f;
            bop.startBeat = beat + 1f;
            unPrepareTime = Single.MinValue;
            lastChargeTime = Single.MinValue;
            inKick = false;

            anim.DoScaledAnimationAsync("ManKick", 0.5f);
        }

        public void MarkCanEmote()
        {
            canEmote = true;
        }

        public void MarkNoEmote()
        {
            canEmote = false;
        }

        public void UpdateShadowColour()
        {
            foreach (var shadow in Shadows)
            {
                shadow.color = KarateMan.instance.GetShadowColor();
            }
        }

        public void Prepare(float beat, float length)
        {
            anim.speed = 0f;
            anim.Play("Beat", -1, 0);
            unPrepareTime = beat + length;
        }

        public void SetFaceExpressionForced(int face)
        {
            wantFace = -2;
            FaceAnim.DoScaledAnimationAsync("Face" + face.ToString("D2"));
        }

        public void SetFaceExpression(int face, bool ignoreCheck = false)
        {
            wantFace = face;
            if (canEmote || ignoreCheck)
                FaceAnim.DoScaledAnimationAsync("Face" + face.ToString("D2"));
        }
    }
}