using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{

    #region Static Functions
    /// <summary>
    /// Move transform over time
    /// </summary>
    /// <param name="_transform"></param>
    /// <param name="targetPosition"></param>
    /// <param name="duration"></param>
    /// <param name="smoothing"></param>
    /// <returns></returns>
    public static IEnumerator MoveObjectOverTimeSequence(Transform _transform, Vector3 targetPosition, float duration, float smoothing = 1)
    {
        float timer = 0;
        Vector3 startingPosition = _transform.position;

        while (timer < duration)
        {
            _transform.position = Vector3.Lerp(startingPosition, targetPosition, Mathf.Pow(timer / duration, smoothing));

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _transform.position = targetPosition;
    }


    /// <summary>
    /// Remainder from diving entryValue by modulus
    /// </summary>
    /// <param name="entryValue"></param>
    /// <param name="modulus"></param>
    /// <returns></returns>
    public static float FloatMod(float entryValue, float modulus)
    {
        float newVal = entryValue;
        float mod = Mathf.Abs(modulus);
        if (modulus == 0) return entryValue;
        else if (newVal > 0) while (newVal > mod) newVal -= mod;
        else while (Mathf.Abs(newVal) > mod) newVal += mod;

        return newVal;
    }

    /// <summary>
    /// How many times modulus is contained in entryValue
    /// </summary>
    /// <param name="entryValue"></param>
    /// <param name="modulus"></param>
    /// <param name=""></param>
    /// <returns></returns>
    public static int FloatDiv(float entryValue, float modulus)
    {
        float newVal = entryValue;
        int counter = 0;
        float mod = Mathf.Abs(modulus);
        if (modulus == 0) return int.MaxValue;
        else if (newVal > 0)
            while (newVal > mod)
            {
                newVal -= mod;
                counter++;
            }
        else
            while (Mathf.Abs(newVal) > mod)
            {
                newVal += mod;
                counter++;
            } 

        return counter;
    }

    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }

    /// <summary>
    /// 0 degrees is (1,0), 90 degrees is (0,1)
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }

    public static float Vector2ToDegree(Vector2 vector)
    {
        return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
    }

    public static Texture2D TextureFromSprite(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                         (int)sprite.textureRect.y,
                                                         (int)sprite.textureRect.width,
                                                         (int)sprite.textureRect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
            return sprite.texture;
    }


    public static float RoundZeroOneMinusOne(float valueToRound)
    {
        if (valueToRound > 0) return 1;
        else if (valueToRound < 0) return -1;
        else return 0;
    }

    public static RaycastHit2D[] RaycastSalvo(Vector2 startPos, Vector2 endPos, Vector2 direction, float rayLength, int rayCount, LayerMask collisionMask, float sideInwardOffset = 0)
    {
        RaycastHit2D[] raycasts = new RaycastHit2D[rayCount];

        Vector2 offset = (endPos - startPos).normalized * sideInwardOffset;


        for (int i = 0; i < rayCount - 1; i++)
        {
            Vector2 position = Vector2.Lerp(startPos + offset, endPos - offset, i / (float)(rayCount - 1));

            RaycastHit2D ray = Physics2D.Raycast(position, direction, rayLength, collisionMask);

            Debug.DrawRay(position, direction * rayLength, Color.red);
            raycasts[i] = ray;
        }
        raycasts[rayCount - 1] = Physics2D.Raycast(endPos - offset, direction, rayLength, collisionMask); // last ray
        Debug.DrawRay(endPos - offset, direction * rayLength, Color.red);

        return raycasts;
    }

    #endregion

    #region Extensions

    /// <summary>
    /// Extension method that rotates a Vector2 by a specified degree. Negative angle rotates counter-clockwise.
    /// </summary>
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    /// <summary>
    /// Extension method to shuffles all entries in a list
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);//rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Extension method to check if a layer is in a layermask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool Contains(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    /// <summary>
    /// Return 1 if true or -1 if false.
    /// </summary>
    /// <param name="boolean"></param>
    /// <returns></returns>
    public static float ToOneMinusOne(this bool boolean)
    {
        return boolean ? 1 : -1;
    }

    /*Why doesn't this work? Is it deep/shallow copy problem?
    /// <summary>
    /// Adds size to Rect structure.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="size"></param>
    public static void Inflate(this Rect rect, Vector2 size)
    {
        rect.width += size.x;
        rect.height += size.y;
    }
    */

    #endregion

}
