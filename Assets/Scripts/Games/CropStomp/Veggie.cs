using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyBezierCurves;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_CropStomp
{
    public class Veggie : PlayerActionObject
    {
        static float pickedRotationSpeed = -1080f;

        public bool isMole;
        public Sprite[] veggieSprites;
        public Animator moleAnim;
        public SpriteRenderer veggieSprite;
        public Transform veggieTrans;
        public BezierCurve3D curve;
        private BezierCurve3D hitCurve;

        public float targetBeat;
        private float stompedBeat;
        private float pickedBeat;
        private float pickTime = 1f;
        private int veggieState = 0;
        private bool boinked; // Player got barely when trying to pick.

        private float landBeat;

        private Tween squashTween;

        private CropStomp game;

        private void Start()
        {
            game = CropStomp.instance;

            if (!isMole)
            {
                veggieSprite.sprite = veggieSprites[UnityEngine.Random.Range(0, veggieSprites.Length)];
            }
            else
            {
                pickTime = 1.5f;
            }
        }

        private bool gotStomped; // Safeguard in case nested Update() call breaks.
        private void Update()
        {
            if (!game.isMarching)
                return;

            // Veggie missed. Handle missed state.
            if (veggieState == -1)
            {
                MissedUpdate();
                return;
            }

            // Veggie picked. Handle picked state.
            if (veggieState == 2)
            {
                PickedUpdate();
                return;
            }

            var cond = Conductor.instance;

            float normalizedBeat = cond.GetPositionFromMargin(targetBeat, 1f);
            StateCheck(normalizedBeat);
            
            // In ground.
            if (veggieState == 0)
            {
                if (normalizedBeat > Minigame.LateTime())
                {
                    veggieState = -1;
                    return;
                }

                if (PlayerInput.Pressed())
                {
                    if (state.perfect)
                    {
                        StompVeggie(false);
                    }
                    else if (state.notPerfect())
                    {
                        veggieState = -1;
                    }
                }
            }
            // In air.
            else if (veggieState == 1)
            {
                float airPosition = cond.GetPositionFromBeat(stompedBeat, landBeat - stompedBeat);
                veggieTrans.position = curve.GetPoint(Mathf.Clamp(airPosition, 0, 1));

                if (normalizedBeat > Minigame.EndTime())
                {
                    veggieState = -1;
                    
                    if (!isMole)
                        Jukebox.PlayOneShotGame("cropStomp/veggieMiss");

                    return;
                }

                if (PlayerInput.PressedUp())
                {
                    if (state.perfect)
                    {
                        PickVeggie(false);
                    }
                    else if (state.notPerfect())
                    {
                        veggieState = -1;
                        boinked = true;

                        curve.transform.localScale = Vector3.one; // Return curve to normal size in the case of mole curves.

                        var key1 = curve.KeyPoints[0];
                        var key1Pos = key1.Position;
                        key1.Position = new Vector3(key1Pos.x, veggieTrans.position.y, key1Pos.z);

                        var key2 = curve.KeyPoints[1];
                        var key2Pos = key2.Position;
                        key2.Position = new Vector3(key2Pos.x, veggieTrans.position.y + 2f, key2Pos.z);

                        pickedBeat = cond.songPositionInBeats;

                        Jukebox.PlayOneShot("miss");

                        MissedUpdate();
                    }

                    game.bodyAnim.Play("Pick", 0, 0);
                    game.isFlicking = true;
                }
            }
        }

        bool moleLaughing;
        private void MissedUpdate()
        {
            if (boinked)
            {
                float fallPosition = Conductor.instance.GetPositionFromBeat(pickedBeat, 1f);
                fallPosition = Mathf.Clamp(fallPosition, 0, 1);
                veggieTrans.position = curve.GetPoint(fallPosition);

                if (fallPosition < 1f)
                {
                    var rotSpeed = isMole ? pickedRotationSpeed : -pickedRotationSpeed;
                    veggieTrans.rotation = Quaternion.Euler(0, 0, veggieTrans.rotation.eulerAngles.z + (rotSpeed * Time.deltaTime));
                }
                else
                {
                    veggieTrans.rotation = Quaternion.Euler(0, 0, 180f);
                }
            }
            else
            {
                if (isMole && !moleLaughing)
                {
                    var distDiff = transform.position.x - game.farmerTrans.position.x;
                    if (distDiff > 1.5f)
                    {
                        moleAnim.Play("Chuckle", 0, 0);
                        moleLaughing = true;
                    }
                }
            }
        }

        private void PickedUpdate()
        {
            float pickPosition = Conductor.instance.GetPositionFromBeat(pickedBeat, pickTime);
            pickPosition = Mathf.Clamp(pickPosition, 0, 1);
            veggieTrans.position = hitCurve.GetPoint(pickPosition);

            var rotSpeed = isMole ? -pickedRotationSpeed : pickedRotationSpeed;
            veggieTrans.rotation = Quaternion.Euler(0, 0, veggieTrans.rotation.eulerAngles.z + (rotSpeed * Time.deltaTime));

            if (!isMole)
            {
                var veggieScale = Mathf.Min(1.5f - pickPosition, 1f);
                veggieTrans.localScale = Vector2.one * veggieScale;
            }
        }

        private void StompVeggie(bool autoTriggered)
        {
            // Juuuuuust in case.
            if (gotStomped)
            {
                Debug.Log("Recursion moment?");
                return;
            }
            gotStomped = true;

            var cond = Conductor.instance;

            veggieState = 1;
            targetBeat = targetBeat + (isMole ? 0.5f : 1f);

            stompedBeat = cond.songPositionInBeats;

            landBeat = cond.GetBeatFromPositionAndMargin(Minigame.EndTime(), targetBeat, 1f);

            if (autoTriggered)
            {
                game.Stomp();
                game.bodyAnim.Play("Stomp", 0, 0);
            }

            if (!isMole)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(targetBeat - 0.5f, delegate { Jukebox.PlayOneShotGame("cropStomp/veggieOh"); })
                });
            }
            else
            {
                moleAnim.Play("Idle", 0, 0);
            }

            var veggieScale = veggieTrans.localScale;
            veggieTrans.localScale = new Vector3(veggieScale.x * 0.5f, veggieScale.y, veggieScale.z);
            squashTween = veggieTrans.DOScaleX(veggieScale.x, cond.pitchedSecPerBeat * 0.5f);

            ResetState();

            Update(); // Update flying veggie state immediately.
        }

        private void PickVeggie(bool autoTriggered)
        {
            veggieState = 2;

            if (autoTriggered)
            {
                game.bodyAnim.Play("Pick", 0, 0);
                game.isFlicking = true;
            }
            
            var key1 = game.pickCurve.KeyPoints[0];
            var keyPos = key1.Position;
            key1.Position = new Vector3(keyPos.x, veggieTrans.position.y, keyPos.z);

            pickedBeat = Conductor.instance.songPositionInBeats;

            if (!isMole)
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(pickedBeat + 0.5f, delegate { veggieSprite.sortingOrder = -1; }),
                    new BeatAction.Action(pickedBeat + pickTime, delegate { GameObject.Destroy(gameObject); })
                });

                Jukebox.PlayOneShotGame("cropStomp/veggieKay");

                hitCurve = game.pickCurve;
            }
            else
            {
                BeatAction.New(gameObject, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(pickedBeat + pickTime, delegate { GameObject.Destroy(gameObject); })
                });

                Jukebox.PlayOneShotGame("cropStomp/GEUH");

                hitCurve = game.moleCurve;
            }

            if (squashTween != null)
                squashTween.Kill(true);

            PickedUpdate();
        }

        public override void OnAce()
        {
            if (veggieState == 0)
                StompVeggie(true);
            else
                PickVeggie(true);
        }
    }
}
