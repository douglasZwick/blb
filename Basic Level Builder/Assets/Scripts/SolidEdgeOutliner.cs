using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes outlines appear around tiles.
/// </summary>
public class SolidEdgeOutliner : MonoBehaviour
{
  // A reference to the TileGrid is needed to access this tile's neighbors
  static TileGrid s_TileGrid;
  // The outline sprites are loaded at Awake-time and stored here
  static Dictionary<int, Sprite> s_SquareSprites;
  static Dictionary<int, Sprite> s_LDtoRUSlopeSprites;
  static Dictionary<int, Sprite> s_LUtoRDSlopeSprites;

  // The outline renderer, whose sprite is set during Setup
  public SpriteRenderer m_SpriteRenderer;
  // Does this tile have a solid right side?
  public bool m_SolidRight = true;
  // Does this tile have a solid top side?
  public bool m_SolidUp = true;
  // Does this tile have a solid left side?
  public bool m_SolidLeft = true;
  // Does this tile have a solid bottom side?
  public bool m_SolidDown = true;

  // Is this tile currently being erased? (Set to true in Erase)
  bool m_BeingErased = false;


  // Sets up the outline.
  // Call this when you've just created this tile
  public void Setup(Vector2Int index)
  {
    // Farm out the hard work to a helper
    SetupHelper(index, true);
  }


  // Sets the m_BeingErased flag and then sets up the neighbors.
  // Call this when you're about to erase this tile
  public void Erase(Vector2Int index)
  {
    m_BeingErased = true;
    SetupHelper(index, true);
  }


  // Does the actual work of setting up the outline.
  // - Vector2Int index: the index of the tile being set up
  // - bool setUpNeighbors: whether this function should also be called
  //                        on this tile's neighbors
  void SetupHelper(Vector2Int index, bool setUpNeighbors)
  {
    // Note: there are some redundancies throughout this process, mostly
    // in cases where avoiding them would be a huge hassle resulting in
    // code that's much more difficult to read. Nothing about this algorithm
    // is particularly computationally expensive, so it's unlikely that
    // any benefit would be gained at all from "fixing" these redundancies.

    var code = 0;
    // This code will be the index to use to retrieve the appropriate
    // outline sprite from the appropriate dictionary. Each bit in this
    // code corresponds to a portion of the outline, either a corner or
    // an edge:
    //
    //      2----5----1
    //      |         |
    //      6         4
    //      |         |
    //      3----7----0
    //
    // The code starts out at 0 (indicating no outline at all), but its
    // bits are turned on based on various checks to determine whether
    // that part of the outline should be active.

    // A code of 0 is a suitable default for an ordinary square, but
    // non-square tiles, such as diagonal slopes, should have different
    // starting bit patterns.
    // 
    // If other tile shapes are added later, this system should accommodate
    // them properly, but additional ASCII drawings should be added as well.
    // For now, the drawings here will reflect only the shapes that we
    // already have.

    // No solid right means either
    //   +        +-+
    //   |\   or  |/
    //   +-+      +
    if (!m_SolidRight)
    {                       //                 +
      if (m_SolidDown)      //                 |\
        code |= 0b00000001; //                 +-0 <- set this bit

      if (m_SolidUp)        //                 +-1 <- set this bit
        code |= 0b00000010; //                 |/
    }                       //                 +

    // No solid up means either
    //     +      +
    //    /|  or  |\
    //   +-+      +-+
    if (!m_SolidUp)
    {                       //                   1 <- set this bit
      if (m_SolidRight)     //                  /|
        code |= 0b00000010; //                 +-+

      if (m_SolidLeft)      // set this bit -> 2
        code |= 0b00000100; //                 |\
    }                       //                 +-+

    // No solid left means either
    //   +-+        +
    //    \|  or   /|
    //     +      +-+
    if (!m_SolidLeft)
    {                       // set this bit -> 2-+
      if (m_SolidUp)        //                  \|
        code |= 0b00000100; //                   +

      if (m_SolidDown)      //                   +
        code |= 0b00001000; //                  /|
    }                       // set this bit -> 3-+

    // No solid down means either
    //   +-+      +-+
    //   |/   or   \|
    //   +          +
    if (!m_SolidDown)
    {                       //                 +-+
      if (m_SolidLeft)      //                 |/
        code |= 0b00001000; // set this bit -> 3

      if (m_SolidRight)     //                 +-+
        code |= 0b00000001; //                  \|
    }                       //                   1 <- set this bit

    // So far, all the setup we've done to prepare the code value has been based
    // on data present on this object itself, but now it's time to start looking
    // at our neighbors.

    // First we compute indices for all the neighboring grid elements
    var rdI = index + Vector2Int.right + Vector2Int.down;
    var ruI = index + Vector2Int.right + Vector2Int.up;
    var luI = index + Vector2Int.left + Vector2Int.up;
    var ldI = index + Vector2Int.left + Vector2Int.down;
    var rI = index + Vector2Int.right;
    var uI = index + Vector2Int.up;
    var lI = index + Vector2Int.left;
    var dI = index + Vector2Int.down;

    // Then we get the elements themselves from those indices
    var rd = s_TileGrid.Get(rdI);
    var ru = s_TileGrid.Get(ruI);
    var lu = s_TileGrid.Get(luI);
    var ld = s_TileGrid.Get(ldI);
    var r = s_TileGrid.Get(rI);
    var u = s_TileGrid.Get(uI);
    var l = s_TileGrid.Get(lI);
    var d = s_TileGrid.Get(dI);

    // Then we grab all the SolidEdgeOutliners of those elements, if any.
    // TileGrid.Get returns an empty element, not null, if there's nothing
    // at the specified index, so it's safe to check its m_GameObject field,
    // but of course you wanna use the ?. to check it, because if it's an
    // empty element, then m_GameObject will be null, and then you're hosed
    var rdO = rd.m_GameObject?.GetComponent<SolidEdgeOutliner>();
    var ruO = ru.m_GameObject?.GetComponent<SolidEdgeOutliner>();
    var luO = lu.m_GameObject?.GetComponent<SolidEdgeOutliner>();
    var ldO = ld.m_GameObject?.GetComponent<SolidEdgeOutliner>();
    var rO = r.m_GameObject?.GetComponent<SolidEdgeOutliner>();
    var uO = u.m_GameObject?.GetComponent<SolidEdgeOutliner>();
    var lO = l.m_GameObject?.GetComponent<SolidEdgeOutliner>();
    var dO = d.m_GameObject?.GetComponent<SolidEdgeOutliner>();

    // Now that we have each neighboring outliner, we can proceed. The
    // overall idea here is that a bit should be set if the neighbor in the
    // corresponding direction is either not solid "toward" this tile, or the
    // equivalent thereof (e.g., it's null, etc.). Beyond that, experimentation
    // has revealed that there are a handful of additional conditions that
    // warrant turning on a bit, and those are checked here as well.
    // 
    // Additionally, after looking at each neighbor and potentially modifying the
    // code value based on it, we also need to call SetupHelper on that neighbor
    // so that it has a chance to make its outline look right based on this tile
    // that is now being set up. Of course, in this case, SetupHelper is called
    // with its own setUpNeighbors flag false, so that we don't set this tile up
    // again and then set up the neighbor again and so on infinitely.
                                                                                        // +----+.....
    if (rdO == null || rdO.m_BeingErased || (!rdO.m_SolidLeft || !rdO.m_SolidUp) ||     // |this| rO .
      (rO != null && rO.m_SolidLeft && !rO.m_SolidDown) ||                              // +----+.....
      (dO != null && dO.m_SolidUp && !dO.m_SolidRight))                                 // . dO .rdO .
      code |= 0b00000001;                                                               // ...........
    // Set up rdO if appropriate
    else if (rdO != null && setUpNeighbors)
      rdO.SetupHelper(rdI, false);
                                                                                        // ...........
    if (ruO == null || ruO.m_BeingErased || (!ruO.m_SolidLeft || !ruO.m_SolidDown) ||   // . uO .ruO .
      (rO != null && rO.m_SolidLeft && !rO.m_SolidUp) ||                                // +----+.....
      (uO != null && uO.m_SolidDown && !uO.m_SolidRight))                               // |this| rO .
      code |= 0b00000010;                                                               // +----+.....
    // Set up ruO if appropriate
    else if (ruO != null && setUpNeighbors)                                           
      ruO.SetupHelper(ruI, false);
                                                                                        // ...........
    if (luO == null || luO.m_BeingErased || (!luO.m_SolidRight || !luO.m_SolidDown) ||  // .luO . uO .
      (lO != null && lO.m_SolidRight && !lO.m_SolidUp) ||                               // .....+----+
      (uO != null && uO.m_SolidDown && !uO.m_SolidLeft))                                // . lO |this|
      code |= 0b00000100;                                                               // .....+----+
    // Set up luO if appropriate
    else if (luO != null && setUpNeighbors)
      luO.SetupHelper(luI, false);
                                                                                        // .....+----+
    if (ldO == null || ldO.m_BeingErased || (!ldO.m_SolidRight || !ldO.m_SolidUp) ||    // . lO |this|
      (lO != null && lO.m_SolidRight && !lO.m_SolidDown) ||                             // .....+----+
      (dO != null && dO.m_SolidUp && !dO.m_SolidLeft))                                  // .ldO . dO .
      code |= 0b00001000;                                                               // ...........
    // Set up ldO if appropriate
    else if (ldO != null && setUpNeighbors)
      ldO.SetupHelper(ldI, false);
                                                                                        // 
    if (rO == null || rO.m_BeingErased || !rO.m_SolidLeft)                              // +----+.....
      code |= 0b00010011;                                                               // |this| rO .
    // Set up rO if appropriate                                                         // +----+.....
    else if (rO != null && setUpNeighbors)                                              // 
      rO.SetupHelper(rI, false);
                                                                                        //   ......
    if (uO == null || uO.m_BeingErased || !uO.m_SolidDown)                              //   . uO .
      code |= 0b00100110;                                                               //   +----+
    // Set up uO if appropriate                                                         //   |this|
    else if (uO != null && setUpNeighbors)                                              //   +----+
      uO.SetupHelper(uI, false);
                                                                                        // 
    if (lO == null || lO.m_BeingErased || !lO.m_SolidRight)                             // .....+----+
      code |= 0b01001100;                                                               // . lO |this|
    // Set up lO if appropriate                                                         // .....+----+
    else if (lO != null && setUpNeighbors)                                              // 
      lO.SetupHelper(lI, false);
                                                                                        //   +----+
    if (dO == null || dO.m_BeingErased || !dO.m_SolidUp)                                //   |this|
      code |= 0b10001001;                                                               //   +----+
    // Set up dO if appropriate                                                         //   . dO .
    else if (dO != null && setUpNeighbors)                                              //   ......
      dO.SetupHelper(dI, false);

    // Now we're done with our neighbors, and it's time for more
    // introspection. We're going to be turning bits off this time. A
    // slope tile without a solid top, for example, should certainly
    // not have the top of its outline drawn, regardless of what's
    // going on with its neighbors. We also want to turn corner bits
    // off for similar reasons.
    // 
    // Here's this diagram, once again:
    // 
    //      2----5----1
    //      |         |
    //      6         4
    //      |         |
    //      3----7----0

    if (!m_SolidRight)    // No solid right means turn off bit 4
    {
      code &= ~(0b00010000);

      if (!m_SolidDown)   // No solid right && no solid down means turn off bit 0
        code &= ~(0b00000001);
      if (!m_SolidUp)     // No solid right && no solid up means turn off bit 1
        code &= ~(0b00000010);
    }

    if (!m_SolidUp)       // No solid up means turn off bit 5
    {
      code &= ~(0b00100000);

      if (!m_SolidRight)  // No solid up && no solid right means turn off bit 1
        code &= ~(0b00000010);
      if (!m_SolidLeft)   // No solid up && no solid left means turn off bit 2
        code &= ~(0b00000100);
    }

    if (!m_SolidLeft)     // No solid left means turn off bit 6
    {
      code &= ~(0b01000000);

      if (!m_SolidUp)     // No solid left && no solid up means turn off bit 2
        code &= ~(0b00000100);
      if (!m_SolidDown)   // No solid left && no solid down means turn off bit 3
        code &= ~(0b00001000);
    }

    if (!m_SolidDown)     // No solid down means turn off bit 7
    {
      code &= ~(0b10000000);

      if (!m_SolidLeft)   // No solid down && no solid left means turn off bit 3
        code &= ~(0b00001000);
      if (!m_SolidRight)  // No solid down && no solid right means turn off bit 0
        code &= ~(0b00000001);
    }

    // Oookay. One last bit of potential code manipulation:
    // 
    // The outline on every tile is an interior stroke -- that is,
    // it doesn't spill out of the visible area of the tile itself.
    // (This is true whether it's a solid square tile or any other
    // shape.)
    // 
    // Now, the placement of the outline pixels within the sprites
    // themselves is in most cases entirely determinable by the
    // bits that have been set, and thus by the code value
    // determined thereby; that is, in a slope tile with, say, bits
    // 0, 1, 3, and 4, set, it unambiguously specifies the shape:
    //     
    //                  +
    //                / |
    //              /   |
    //            +     +
    //     
    // However, when the bits get set like this:
    //     
    //           +        +
    //         /     or     \
    //       /                \
    //     +                    +
    //     
    // ...is this a ceiling? Or is it a floor? You can't tell by
    // the code bits alone (unlike in any other case). And it
    // matters, because the outline pixels of the slope part of a
    // ceiling slope tile are not in the same part of the sprite
    // as the outline pixels of the slope part of a floor slope.
    // 
    // So, to handle this situation, which comes about only when
    // this is a slope tile where no bits got set in checking
    // neighbors (and no bits got cleared afterward), we determine
    // whether this is a ceiling tile based on the solidity flags
    // and then use the special code value of -1 if it's a ceiling.

    // So, to recap all of that:
    //   IF:
    //      - it's a slope tile, determined opposite sides having
    //        opposite solidities, AND
    //      - it's a ceiling slope, determined by the top being
    //        solid, AND
    //      - the code value is unmodified from its initial state,
    //        so it's either 0000 0101 or 0000 1010,
    //   THEN:
    //      - we set the code to be the special code value of -1.
    if (m_SolidUp && !m_SolidDown && m_SolidRight != m_SolidLeft &&
      (code == 0b00000101 || code == 0b00001010))
      code = -1;

    // Now that all of the fuss about the code is through, it's
    // time to interpret it and use it to get the appropriate
    // sprite for the outline renderer.

    // First of all, if the code is 0, then there's no outline
    // at all, and so we should just turn the damn thing off
    if (code == 0)
    {
      m_SpriteRenderer.enabled = false;
    }
    else
    {
      // Otherwise, the renderer should be enabled
      m_SpriteRenderer.enabled = true;

      // We'll use the solidity flags to determine which dictionary
      // to look in to find the correct outline sprite
      Dictionary<int, Sprite> sprites = null;

      // If all four sides are solid, then it's a square
      if (m_SolidRight && m_SolidUp && m_SolidLeft && m_SolidDown)
      {
        sprites = s_SquareSprites;
      }
      // Otherwise, if opposite sides have opposite solidities,
      // then it's one of the two kinds of slope tile
      else if (m_SolidRight != m_SolidLeft && m_SolidUp != m_SolidDown)
      {
        // If the right side has the same solidity as the bottom,
        // then it's a left-down-to-right-up slope
        if (m_SolidRight == m_SolidDown)
          sprites = s_LDtoRUSlopeSprites;
        // Otherwise, it's a left-up-to-right-down slope
        else
          sprites = s_LUtoRDSlopeSprites;
      }

      // Finally, we index the chosen dictionary and assign the
      // sprite we get from it to the outline renderer.
      m_SpriteRenderer.sprite = sprites[code];
    }
  }


  // Loads the sprites and puts them in the dictionaries. Should be called
  // exactly once, in the SolidEdgeOutlineLoader's Awake function.
  static public void Load()
  {
    // Finds and stores a reference to the TileGrid
    s_TileGrid = FindObjectOfType<TileGrid>();

    // Grabs the square outline sprites
    var sprites = LoadSpritesToArray("Sprites/SquareOutlines");

    // Puts each square outline sprite in its corresponding dictionary,
    // keyed with its appropriate code value
    s_SquareSprites = new Dictionary<int, Sprite>()
    {
      { 0x01, sprites[ 0] }, { 0x02, sprites[ 1] }, { 0x03, sprites[ 2] }, { 0x04, sprites[ 3] },
      { 0x05, sprites[ 4] }, { 0x06, sprites[ 5] }, { 0x07, sprites[ 6] }, { 0x08, sprites[ 7] },
      { 0x09, sprites[ 8] }, { 0x0A, sprites[ 9] }, { 0x0B, sprites[10] }, { 0x0C, sprites[11] },
      { 0x0D, sprites[12] }, { 0x0E, sprites[13] }, { 0x0F, sprites[14] }, { 0x13, sprites[15] },
      { 0x17, sprites[16] }, { 0x1B, sprites[17] }, { 0x1F, sprites[18] }, { 0x26, sprites[19] },
      { 0x27, sprites[20] }, { 0x2E, sprites[21] }, { 0x2F, sprites[22] }, { 0x37, sprites[23] },
      { 0x3F, sprites[24] }, { 0x4C, sprites[25] }, { 0x4D, sprites[26] }, { 0x4E, sprites[27] },
      { 0x4F, sprites[28] }, { 0x5F, sprites[29] }, { 0x6E, sprites[30] }, { 0x6F, sprites[31] },
      { 0x7F, sprites[32] }, { 0x89, sprites[33] }, { 0x8B, sprites[34] }, { 0x8D, sprites[35] },
      { 0x8F, sprites[36] }, { 0x9B, sprites[37] }, { 0x9F, sprites[38] }, { 0xAF, sprites[39] },
      { 0xBF, sprites[40] }, { 0xCD, sprites[41] }, { 0xCF, sprites[42] }, { 0xDF, sprites[43] },
      { 0xEF, sprites[44] }, { 0xFF, sprites[45] },
    };

    // Grabs the left-down-to-right-up slope outline sprites
    sprites = LoadSpritesToArray("Sprites/LDtoRUSlopeOutlines");

    // Puts each left-down-to-right-up slope outline sprite in its
    // corresponding dictionary, keyed with its appropriate code value
    s_LDtoRUSlopeSprites = new Dictionary<int, Sprite>()
    {
      { 0x0A, sprites[0] },
      { 0x0B, sprites[1] },
      { 0x0E, sprites[2] },
      { 0x1B, sprites[3] },
      { 0x2E, sprites[4] },
      { 0x4E, sprites[5] },
      { 0x6E, sprites[6] },
      { 0x8B, sprites[7] },
      { 0x9B, sprites[8] },

      { -1, sprites[9] }  // See above wall of text for what -1 is all about
    };

    // Grabs the right-down-to-left-up slope outline sprites
    sprites = LoadSpritesToArray("Sprites/LUtoRDSlopeOutlines");

    // Puts each right-down-to-left-up slope outline sprite in its
    // corresponding dictionary, keyed with its appropriate code value
    s_LUtoRDSlopeSprites = new Dictionary<int, Sprite>()
    {
      { 0x05, sprites[0] },
      { 0x07, sprites[1] },
      { 0x0D, sprites[2] },
      { 0x17, sprites[3] },
      { 0x27, sprites[4] },
      { 0x37, sprites[5] },
      { 0x4D, sprites[6] },
      { 0x8D, sprites[7] },
      { 0xCD, sprites[8] },

      { -1, sprites[9] }  // See above wall of text for what -1 is all about
    };
  }


  // Loads the sprites at the specified path.
  // - string path: the path to the sprites. May be either a
  //                folder or a single sliced sprite
  // return Sprites[]: the loaded sprites
  static Sprite[] LoadSpritesToArray(string path)
  {
    // Resources.LoadAll can only return Object[], regardless of
    // what you specify in its second parameter. Honestly, I
    // wonder why I even try sometimes
    var objects = Resources.LoadAll(path, typeof(Sprite));
    // And there doesn't seem to be any way of converting an
    // Object[] into a Sprite[], so we have to declare this
    // sprite array and then walk through and dump them all in
    var sprites = new Sprite[objects.Length];

    for (var i = 0; i < objects.Length; ++i)
      sprites[i] = objects[i] as Sprite;

    return sprites;
  }
}
