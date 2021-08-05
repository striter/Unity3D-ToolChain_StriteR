using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

public class AtlasLoader
{
    protected Dictionary<string, Sprite> m_SpriteDic { get; private set; } = new Dictionary<string, Sprite>();
    public bool Contains(string name) => m_SpriteDic.ContainsKey(name);
    public string m_AtlasName { get; private set; }
    public Sprite this[string name]
    {
        get
        {
            if (!m_SpriteDic.ContainsKey(name))
            {
                Debug.LogWarning("Null Sprites Found |" + name + "|" + m_AtlasName);
                return m_SpriteDic.Values.First();
            }
            return m_SpriteDic[name];
        }
    }
    public AtlasLoader(SpriteAtlas atlas)
    {
        m_AtlasName = atlas.name;
        Sprite[] allsprites = new Sprite[atlas.spriteCount];
        atlas.GetSprites(allsprites);
        foreach (Sprite sprite in allsprites)
        {
            string name = sprite.name.Replace("(Clone)", ""); 
            m_SpriteDic.Add(name, sprite); 
        }
    }
}

public class AtlasAnim : AtlasLoader
{
    int animIndex = 0;
    List<Sprite> m_Anims;
    public AtlasAnim(SpriteAtlas atlas) : base(atlas)
    {
        m_Anims = m_SpriteDic.Values.ToList();
        m_Anims.Sort((a, b) =>
        {
            int index1 = int.Parse(System.Text.RegularExpressions.Regex.Replace(a.name, @"[^0-9]+", ""));
            int index2 = int.Parse(System.Text.RegularExpressions.Regex.Replace(b.name, @"[^0-9]+", ""));
            return index1 - index2;
        });
    }

    public Sprite Reset()
    {
        animIndex = 0;
        return m_Anims[animIndex];
    }

    public Sprite Tick()
    {
        animIndex++;
        if (animIndex == m_Anims.Count)
            animIndex = 0;
        return m_Anims[animIndex];
    }
}

