namespace VibeBoulderDash.Core;

/// <summary>Semantic tile element as used by the original Boulder Dash engine.</summary>
public enum Element
{
    /// <summary>Empty space, everything can move into it.</summary>
    Space,

    /// <summary>Diggable earth. Supports rocks; Rockford can dig through it.</summary>
    Dirt,

    /// <summary>Brick wall; blocks movement, can be destroyed by explosions.</summary>
    Wall,

    /// <summary>Magic wall that converts falling boulders into diamonds and vice versa while active.</summary>
    MagicWall,

    /// <summary>Indestructible titanium wall.</summary>
    TitaniumWall,

    /// <summary>Boulder Dash II wall that expands horizontally or vertically when hit.</summary>
    ExpandingWall,

    /// <summary>Cave exit; passable only once the required diamonds are collected.</summary>
    Exit,

    /// <summary>The player character Rockford.</summary>
    Rockford,

    /// <summary>Boulder with gravity; falls and slides, can crush Rockford.</summary>
    Boulder,

    /// <summary>Diamond with gravity; collected by Rockford or created on demand.</summary>
    Diamond,

    /// <summary>Firefly: glides along walls, explodes on contact.</summary>
    Firefly,

    /// <summary>Butterfly: glides opposite to fireflies, turns into diamonds when crushed.</summary>
    Butterfly,

    /// <summary>Amoeba: grows through dirt and space, suffocates into diamonds.</summary>
    Amoeba,

    /// <summary>Boulder Dash II creeping goo that grows through and dissolves the ground.</summary>
    Slime,
}

/// <summary>Element classification helpers.</summary>
public static class ElementExtensions
{
    /// <summary>Whether this element blocks falling boulders and diamonds (i.e. is solid).</summary>
    public static bool IsSolid(this Element element) => element switch
    {
        Element.Space => false,
        Element.Rockford => false,
        Element.Firefly => false,
        Element.Butterfly => false,
        Element.Amoeba => false,
        _ => true,
    };

    /// <summary>Whether this element is a gravity element (boulder or diamond).</summary>
    public static bool IsGravity(this Element element) =>
        element is Element.Boulder or Element.Diamond;

    /// <summary>Whether this element is a flying creature (firefly or butterfly).</summary>
    public static bool IsFly(this Element element) =>
        element is Element.Firefly or Element.Butterfly;
}