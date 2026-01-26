# Quick Start - AI Training

## Bước 1: Cài Đặt ML-Agents

Thêm vào `Packages/manifest.json`:
```json
"com.unity.ml-agents": "2.0.0"
```

## Bước 2: Setup Scene

1. Mở scene `DemoGame.unity`
2. Tạo GameObject "AIAgent" → Add Component `DemoAIAgent`
3. Tạo GameObject "AIGameController" → Add Component `DemoAIGameController`
4. Assign references trong Inspector:
   - **AIAgent**: 
     - Game Controller: `DemoGameController`
     - Shapes: [Cube, Sphere, Capsule]
   - **AIGameController**:
     - Game Controller: `DemoGameController`
     - AI Agent: `AIAgent`
     - Use AI: ✅ (bật lên)

## Bước 3: Cấu Hình Behavior Parameters

Trên GameObject `AIAgent`, Unity sẽ tự động thêm `Behavior Parameters`:
- **Behavior Name**: `DemoGameAgent`
- **Vector Observation** → **Space Size**: `20`
- **Actions** → **Discrete Actions**:
  - **Branches Size**: `1`
  - **Branch 0 Size**: `4`

## Bước 4: Train

```bash
# Cài đặt mlagents (nếu chưa có)
pip install mlagents

# Train
mlagents-learn Assets/ThirdParties/Zeta/ProjectAnalysis/Demo/ML-Agents/demo_game_config.yaml --run-id=demo_game_v1
```

## Bước 5: Sử Dụng Model

1. Sau khi train xong, copy file `.onnx` từ `results/<run-id>/` vào `Assets/.../ML-Agents/Models/`
2. Trong Unity, chọn `AIAgent` → `Behavior Parameters` → **Model**: chọn file `.onnx`
3. Chạy scene → AI sẽ tự động chơi!

## Test Heuristic (Không cần train)

Để test nhanh, trong `Behavior Parameters`:
- **Behavior Type**: `Heuristic Only`
- AI sẽ chọn đúng theo thứ tự (perfect player)

---

Xem [README_ML_AGENTS.md](README_ML_AGENTS.md) để biết chi tiết đầy đủ.
