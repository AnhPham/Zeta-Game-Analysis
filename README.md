# Zeta-Game-Analysis

Recreate user behavior while playing the game to improve UI/UX and level design

## Demo

<p align="center">
  <img width="800px" src="/Learns/Demo.gif?raw=true" alt="Demo">
</p>

---

## Usage

### Requirement

- Import **Newtonsoft.Json** from Unity **Package Manager**.

### Configuration

1. Locate the `zeta_config.json` file in the project and fill in the information provided by the admin.

2. Find the **CoreAPIClient** prefab in the project and drag the `zeta_config.json` file into the **Config File** field.

3. Drag the **CoreAPIClient** prefab into the scene.  
   - If you do not want to use the error/crash logging feature, disable **Log Feature** in the CoreAPIClient.

---

### Sending Behaviour Data

Add the namespace:

```csharp
using Zeta.ProjectAnalysis;
```

#### When the user selects a game element

```csharp
BehaviourAPIClient.Add(
    behaviourId: "element_selected",
    level: 1,
    screen: "game",
    objectId: "3"
);
```

#### When the user clicks a button on the screen

```csharp
BehaviourAPIClient.Add(
    behaviourId: "button_clicked",
    level: 2,
    screen: "home",
    objectId: "settings_button"
);
```

#### When the user clicks but does not touch any element or button

```csharp
BehaviourAPIClient.Add(
    behaviourId: "miss_clicked",
    level: 3,
    screen: "game"
);
```

#### When the user starts / completes / fails a level

```csharp
BehaviourAPIClient.Add(behaviourId: "level_started", level: 4, screen: "game");
BehaviourAPIClient.Add(behaviourId: "level_completed", level: 4, screen: "game");
BehaviourAPIClient.Add(behaviourId: "level_failed", level: 5, screen: "game");
```

> **Note:** You can use any `behaviourId` or `objectId` that fits your tracking needs.

#### Send data to the Zeta server

```csharp
BehaviourAPIClient.Send();
```

> **Note:** To reduce server load, it is recommended to send data only when a level is completed or failed.

---

### Downloading Behaviour Data

1. Go to **Zeta → Menu → Project**.
2. Select the corresponding project.
3. Click **Behaviour**.

A JSON file will be downloaded.  
This file contains behaviour data from all users in the game.

---

### Replaying Behaviour Data in the Unity Editor

1. Edit the downloaded JSON file:
   - Remove the `[` and `]` at the beginning and end of the file.
   - Remove all data from other users.
   - Keep only the data of the user you want to replay.

2. Deserialize the data:

```csharp
var behaviourData = JsonConvert.DeserializeObject<BehaviourRequest>(json);
```

3. Resize the Game View to match the user’s device resolution:

```csharp
#if UNITY_EDITOR
GameWindowEditor.Resize(behaviourData.width, behaviourData.height);
#endif
```

4. Set the following state to prevent adding or sending data during replay:

```csharp
BehaviourAPIClient.CurrentState = BehaviourAPIClient.State.Replaying;
```

5. Replay user actions based on `behaviourData.behaviours`.

- `behaviourId`, `level`, `screen`, and `objectId` are the values you manually added and sent.
- `(x, y)` tap coordinates, `version`, and `time` are automatically collected and sent by the system.
