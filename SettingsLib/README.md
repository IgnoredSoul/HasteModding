### **Internal SettingsLib**

> [!CAUTION]
> This library is for internal use and built to match my own style of coding and preferences. Expect bugs, inconsistencies, or features that might not make sense outside of that context.

**Internal SettingsLib** (*formerly HastySettings*) is a modding utility that helps you create in-game settings inside the Haste settings menu.

It is heavily inspired by [NaokoAF's HastyControls](https://github.com/NaokoAF/HastyControls/tree/master/Core/Settings) and my own modding workflows and game projects.

> [!NOTE]
> This is **not** a replacement for [HasteSettingLib](https://github.com/HasteModding/HasteSettingLib). This is a personal tool. That said, feel free to use it for your mods — just credit me in the workshop/github if you do.

Every constructor and options struct is fully documented via XML summaries. Intellisense will guide you.

This library is intentionally lightweight and opinionated — it's meant to work with **my** projects first. But if you find it useful: cool. Just remember to **credit** if you use it.


---
<!--------------------------------------------------------------------------------------->

### **Features**

* **Int & Float Sliders**
* **Buttons & Bool Toggles**
* **Enum Dropdowns**
* **Collapsible Groups**

> [!TIP]
> You can nest sliders, buttons, bools, and enums **inside** a collapsible. **You cannot nest collapsibles.**

---
<!--------------------------------------------------------------------------------------->

### **Usage Overview**

All elements are instantiated via constructors.  
Each one comes with optional settings structs like `IntOptions`, `ButtonOptions`, etc.

---
<!--------------------------------------------------------------------------------------->

### **Sliders**

#### Int:

```cs
new HastyInt(HastySetting config, string name, string description, IntOptions options = default);
new HastyInt(HastyCollapsible collapsible, string name, string description, IntOptions options = default);
```

#### IntOptions:

```cs
new IntOptions(
    int min = int.MinValue,
    int max = int.MaxValue,
    int defaultValue = 0,
    Action<int> onApplied = null!,
    Action<int> onLoad = null!
);
```

#### Float:

```cs
new HastyFloat(HastySetting config, string name, string description, FloatOptions options = default);
new HastyFloat(HastyCollapsible collapsible, string name, string description, FloatOptions options = default);
```

#### FloatOptions:

```cs
new FloatOptions(
    float min,
    float max,
    float defaultValue,
    Action<float> onApplied = null!,
    Action<float> onLoad = null!,
    bool whole = false
);
```

---
<!--------------------------------------------------------------------------------------->

### **Dropdowns**

#### Enum:

```cs
new HastyEnum<T>(HastySetting config, string name, string description, EnumOptions<T> options = default);
new HastyEnum<T>(HastyCollapsible collapsible, string name, string description, EnumOptions<T> options = default);
```

#### EnumOptions:

```cs
new EnumOptions<T>(
    T defaultValue,
    Action<T> onApplied = null!,
    Action<T> onLoad = null!,
    IEnumerable<string> choices = null!
);
```

---
<!--------------------------------------------------------------------------------------->

### **Booleans**

#### Bool:

```cs
new HastyBool(HastySetting config, string name, string description, BoolOptions options = default);
new HastyBool(HastyCollapsible collapsible, string name, string description, BoolOptions options = default);
```

#### BoolOptions:

```cs
new BoolOptions(
    string offString = "Off",
    string onString = "On",
    bool defaultValue = false,
    Action<bool> onClicked = null!
);
```

---
<!--------------------------------------------------------------------------------------->

### **Buttons**

#### Button:

```cs
new HastyButton(HastySetting config, string name, string description, ButtonOptions options = default);
new HastyButton(HastyCollapsible collapsible, string name, string description, ButtonOptions options = default);
```

#### ButtonOptions:

```cs
new ButtonOptions(
    string buttonText = "",
    Action onClicked = null!
);
```

---
<!--------------------------------------------------------------------------------------->

### **Collapsible Groups**

```cs
new HastyCollapsible(HastySetting config, string name, string description);
```

Use collapsibles to group related settings visually. You can’t nest collapsibles within collapsibles.

---
<!--------------------------------------------------------------------------------------->

### **Final Words**
```
______   ___   _____  _   _ 
| ___ \ / _ \ |_   _|| \ | |
| |_/ // /_\ \  | |  |  \| |
|  __/ |  _  |  | |  | . ` |
| |    | | | | _| |_ | |\  |
\_|    \_| |_/ \___/ \_| \_/
```

---
<!--------------------------------------------------------------------------------------->

</br>
<h3>Anyway, here's the update journey...</h3></br>
</br>

### Update 6.9.420
- yep cock