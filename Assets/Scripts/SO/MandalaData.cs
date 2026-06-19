using UnityEngine;

public enum Difficulty { Easy, Medium, Hard }

[CreateAssetMenu(menuName = "Mandidi/Mandala Data", fileName = "NewMandalaData")]
public class MandalaData : ScriptableObject {
    public string mandalaName;
    public Difficulty difficulty;
    public Sprite thumbnail;
    public Color[] colors;
    public Sprite mandalaColorWatermark;   // para descarga
    public Sprite mandalaBWWatermark;      // por si se necesita en BYN
    public Sprite mandalaColor;    // referencia oculta, Read/Write enabled
    public Sprite mandalaBW;       // lo que ve el usuario
    public Sprite[] rings; // en orden, ring 0 primero
}