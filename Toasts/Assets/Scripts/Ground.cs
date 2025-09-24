using UnityEngine;

public class Ground : MonoBehaviour
{
    [SerializeField]
    private Texture2D baseTexture;

    private Texture2D cloneTexture;

    private SpriteRenderer spriteRenderer;

    private float _widthWorld, __heightWorld;
    private float _widthPixel, __heightPixel;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        cloneTexture = Instantiate(baseTexture);

        UpdateTexture();

        gameObject.AddComponent<PolygonCollider2D>();
    }

    //World
    public float WidthWorld
    {
        get
        {
            if (_widthWorld == 0)
                _widthWorld = spriteRenderer.bounds.size.x;
            return _widthWorld; 
        }
    }

    public float HeightWorld
    {
        get
        {
            if (__heightWorld == 0)
                __heightWorld = spriteRenderer.bounds.size.y;
            return __heightWorld;
        }
    }

    //Pixel
    public float WidthPixel
    {
        get
        {
            if (_widthPixel == 0)
                _widthPixel = spriteRenderer.sprite.texture.width;
            return _widthPixel;
        }
    }

    public float HeightPixel
    {
        get
        {
            if (__heightPixel == 0)
                __heightPixel = spriteRenderer.sprite.texture.height;
            return __heightPixel;
        }
    }

    void UpdateTexture()
    {
        spriteRenderer.sprite = Sprite.Create(cloneTexture,
            new Rect(0, 0, cloneTexture.width, cloneTexture.height),
            new Vector2(0.5f, 0.5f), 50f);
    }

    Vector2Int World2Pixel(Vector2 pos)
    {
        Vector2Int v = Vector2Int.zero;

        float dx = (pos.x - transform.position.x); 
        float dy = (pos.y - transform.position.y);

        v.x = Mathf.RoundToInt(0.5f * WidthPixel + dx * (WidthPixel / WidthWorld));
        v.y = Mathf.RoundToInt(0.5f * HeightPixel + dy * (HeightPixel / HeightWorld));

        return v;
    }

    public void MakeHole(CircleCollider2D col)
    {
        Vector2Int c = World2Pixel(col.bounds.center);

        int r = Mathf.RoundToInt(col.bounds.size.x * WidthPixel / WidthWorld);

        int px, nx, py, ny, d;

        for (int i = 0; i <= r; i++) {

            d = Mathf.RoundToInt(Mathf.Sqrt(r * r - i * r));

            for(int j = 0; j <= d; j++)
            {
                px = c.x + i;
                nx = c.x - i;
                py = c.y + j;
                ny = c.y - j;

                cloneTexture.SetPixel(px, py, Color.clear);
                cloneTexture.SetPixel(nx, py, Color.clear);
                cloneTexture.SetPixel(px, ny, Color.clear);
                cloneTexture.SetPixel(nx, ny, Color.clear);
            }
        }

        cloneTexture.Apply();
        UpdateTexture();

        gameObject.AddComponent<PolygonCollider2D>();
        Destroy(GetComponent<PolygonCollider2D>());
    }
}
