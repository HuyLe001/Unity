using UnityEngine;
using TMPro; // Bắt buộc để điều khiển TextMeshPro

public class NPC_Talk : MonoBehaviour
{
    [Header("Giao dien hoi thoai")]
    // Ô để bạn kéo cái Panel vào
    public GameObject dialoguePanel;

    // Ô để bạn kéo cái Text vào
    public TextMeshProUGUI dialogueText;

    [Header("Noi dung loi thoai")]
    // Nội dung bạn muốn chú dế nói
    [TextArea(3, 10)]
    public string message = "Chào bạn! Tôi là chú dế thông thái đây.";

    private void Start()
    {
        // Lúc đầu game thì ẩn cái khung đi
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // Khi Player đi vào vùng va chạm (Trigger) của NPC
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Kiểm tra Tag của nhân vật
        {
            if (dialoguePanel != null && dialogueText != null)
            {
                dialoguePanel.SetActive(true);
                dialogueText.text = message;

                // --- TU DONG CAN CHINH CHU KHI CHAM ---
                RectTransform textRect = dialogueText.GetComponent<RectTransform>();

                // 1. Đưa chữ về chính giữa khung xám (Pos 0,0,0)
                textRect.anchoredPosition = Vector2.zero;

                // 2. Khóa góc xoay về 0 để không bị quay theo NPC
                textRect.localRotation = Quaternion.identity;

                // 3. Ép chữ căn giữa theo chiều ngang và dọc
                dialogueText.alignment = TextAlignmentOptions.Center;

                // 4. Đảm bảo kích thước khung chữ đủ rộng để không bị nhảy dòng
                textRect.sizeDelta = new Vector2(600, 100);
            }
        }
    }

    // Khi Player đi ra khỏi vùng va chạm
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }
    }
}