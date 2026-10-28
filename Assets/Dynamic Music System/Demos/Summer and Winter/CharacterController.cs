using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MordiAudio.MusicSystem.Demo
{
    public class CharacterController : MonoBehaviour
    {
        const float WALK_ANIM_FADE_TIME = 0.1f, TURN_SPEED = 200f;

        [Header("Movement settings")]
        public float acceleration;
        public float maxSpeed, friction;

        [Header("Constraints")]
        public Vector2 minPos;
        public Vector2 maxPos;

        [Header("Body parts")]
        public Transform leftArm;
        public Transform rightArm, leftLeg, rightLeg;

        Vector3 velocity;

        float walkAnimT, walkAnimRot;

        void Update() {
            bool isMoving = false;

            if (Input.GetKey(KeyCode.LeftArrow))
                transform.Rotate(new Vector3(0f, -Time.deltaTime * TURN_SPEED, 0f));
            if (Input.GetKey(KeyCode.RightArrow))
                transform.Rotate(new Vector3(0f, Time.deltaTime * TURN_SPEED, 0f));
            if (Input.GetKey(KeyCode.UpArrow)) {
                isMoving = true;
                velocity -= transform.forward * acceleration * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.DownArrow)) {
                isMoving = true;
                velocity += transform.forward * acceleration * Time.deltaTime;
            }

            velocity = Vector3.ClampMagnitude(velocity, maxSpeed * friction);

            transform.position += velocity * Time.deltaTime * friction;

            transform.position = new Vector3(Mathf.Clamp(transform.position.x, minPos.x, maxPos.x), transform.position.y, Mathf.Clamp(transform.position.z, minPos.y, maxPos.y));

            velocity /= (1f + friction * Time.deltaTime);

            if (isMoving) {
                walkAnimT = Mathf.Clamp01(walkAnimT + Time.deltaTime / WALK_ANIM_FADE_TIME);
            } else {
                walkAnimT = Mathf.Clamp01(walkAnimT - Time.deltaTime / WALK_ANIM_FADE_TIME);
            }

            walkAnimRot += Time.deltaTime * 10f;
            if (walkAnimRot > Mathf.PI * 2f)
                walkAnimRot -= Mathf.PI * 2f;

            // Animate arms
            AnimateBodyPart(leftArm, 0f);
            AnimateBodyPart(rightLeg, 0f);
            AnimateBodyPart(rightArm, Mathf.PI);
            AnimateBodyPart(leftLeg, Mathf.PI);
        }

        void AnimateBodyPart(Transform arm, float radiansOffset) {
            if (arm == null)
                return;

            arm.localRotation = Quaternion.Euler(Mathf.Sin(walkAnimRot + radiansOffset) * walkAnimT * 45f, 0f, 0f);
        }
    }
}
