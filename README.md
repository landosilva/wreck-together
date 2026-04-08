# 🏗️💥 Wreck Together

> **Demolition Co-op** — A chaotic 4-player co-op game where you demolish structures, dodge debris, and try not to wreck everything you weren't supposed to.

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![Players](https://img.shields.io/badge/Players-1--4-blue)
![Status](https://img.shields.io/badge/Status-Prototype-orange)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 🎮 Core Concept

**Wreck Together** is a cooperative demolition game for 1–4 players. Work together to tear down structures using sledgehammers, cutters, and explosives — but be careful. Collapse the wrong wall, and you'll flatten your teammates or destroy the protected zones.

> **Spawn → Read Objective → Demolish → Collapse → Success or Fail → Restart**

---

## 🔧 Tools of Destruction

| Tool | Description | Use Case |
|------|-------------|----------|
| 🔨 **Sledgehammer** | Basic melee damage | Smash through normal pieces |
| ✂️ **Cutter** | Removes critical joints | Precision structural takedowns |
| 💣 **Charges** | Timed explosive | Large-scale controlled demolition |

---

## 🧱 Structure System

Buildings are made of modular pieces, each with different roles:

| Piece Type | Behavior |
|------------|----------|
| ⬜ **Normal** | Standard block — breaks easily |
| 🟧 **Structural** | Load-bearing — removing causes shifts |
| 🟥 **Critical** | Keystone — removing triggers major collapse |

---

## 🎯 Objectives & Scoring

### ✅ Win Conditions
- 🏚️ Demolish the target structure
- 🚫 Avoid damaging red (protected) zones
- 📐 Collapse debris within designated bounds

### ❌ Failure Conditions
- 💔 Damage to protected areas
- 🪨 Collapse outside the allowed zone
- 💀 All players downed simultaneously

### 🏆 Scoring
| Factor | Effect |
|--------|--------|
| 🎯 Clean demolition | **+ Bonus** |
| 💥 Collateral damage | **- Penalty** |

---

## 👥 Players

### Co-op (2–4 Players)
The full experience. Coordinate tool usage, revive downed teammates, and carry injured players to safety.

### Solo (1 Player)
Simplified rules — no downed state, only knockdown. All the destruction, none of the teamwork.

---

## 💢 Damage & Player States

```
Small Impact  → 😵‍💫 Stumble (brief animation)
Medium Impact → 🔄 Knockdown (short stun, auto-recover)
Large Impact  → ⬇️ Downed (needs teammate revive)
```

### Player State Machine

```
    ┌──────────┐
    │  Normal  │
    └────┬─────┘
         │
    ┌────▼─────┐    auto
    │ Knockdown ├──────► Normal
    └────┬──────┘
         │
    ┌────▼─────┐    revive/carry
    │  Downed   ├──────────────► Normal
    └──────────┘
```

### Recovery

| Mode | Mechanic |
|------|----------|
| 🤝 **Co-op** | Revive or carry downed players |
| 🧍 **Solo** | No downed state — only knockdown |

---

## 🗺️ Level Design Philosophy

- **Small maps** with clear, focused objectives
- **Strong before/after contrast** — see the impact of your demolition
- Clear visual language for protected vs. target zones

---

## 🧪 Prototype Scope

The initial prototype focuses on the bare essentials:

- [x] 4-player co-op movement
- [x] 1 playable level
- [x] Basic destruction system
- [x] Player downed/revive system
- [ ] Tool switching
- [ ] Scoring system
- [ ] UI/HUD

---

## 🚀 Getting Started

### Prerequisites
- **Unity 2022.3 LTS** or later
- Git LFS (recommended for large assets)

### Setup
```bash
git clone git@github.com:landosilva/wreck-together.git
cd wreck-together
```

Open the project in Unity Hub and let it import.

---

## 🤝 Contributing

This is a prototype — contributions, ideas, and feedback are welcome! Open an issue or submit a PR.

---

## 📄 License

This project is licensed under the **MIT License** — see [LICENSE](LICENSE) for details.

---

<p align="center">
  <b>🏗️ Build it up. 💥 Wreck it together.</b>
</p>
