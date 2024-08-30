# Project's Syntax Ruleset
This file contains all the rules that are to be followed when contributing to the project. Please note that the following list **isn't** exclusive, meaning that it might miss some cases that are left for personal preference. It will likely be updated in the future as time goes on.

For any doubts, please make sure to contact other developers at our [Discord](https://discord.gg/yUF2ec8Vt8).

### Class names:
Will always use "Pascal Case".
```C#
public class ClassName
```

### Class variables:
Will always use "cammel Case".
```C#
public string variableName
```

### Interface names:
Will always start with "I" followed by "Pascal Case".
```C#
public interface IName
```

### Constant variables:
Will always use "full caps".
```C#
public const CONSTANTVARIABLE
```

### Packet specific variables that are sent through the network:
Will always use starting underscore followed by "cammel Case".
```C#
public int _variableName
```

### File specific variables that can be edited by players:
Will always use "Pascal Case".
```C#
public float VariableName
```
