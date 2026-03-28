using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("JumpPad Settings")]
    [SerializeField] float targetJumpHeight = 5f; // กำหนดความสูงที่ต้องการให้เด้งขึ้นไป (เมตร)
    [SerializeField] float gravityValue = 30f;    // ค่า gravity ที่สอดคล้องกับสคริปต์ Character_Move

    private void OnTriggerEnter(Collider other)
    {
        Character_Move player = other.GetComponent<Character_Move>();

        if (player != null)
        {
            // ==========================================
            //  การประยุกต์ใช้ฟิสิกส์: สูตรการเคลื่อนที่ (Kinematics)
            // v^2 = u^2 + 2as -> u = sqrt(2gh)
            // เพื่อคำนวณหาความเร็วต้น (Launch Force) ที่ต้องใช้
            // ==========================================

            float calculatedLaunchForce = Mathf.Sqrt(targetJumpHeight * 2f * gravityValue);

            // ส่งค่าที่คำนวณได้จากสูตรไปยังplayer
            player.Launch(calculatedLaunchForce);

            Debug.Log($"Jump Pad Calculated Force: {calculatedLaunchForce} for Height: {targetJumpHeight}");
        }
    }
}