using UnityEngine;

public class PaperLetter : MonoBehaviour
{
    public enum Letter { D, E, A, B, F }

    [SerializeField] private Letter letter;
    public Letter Value => letter;
}