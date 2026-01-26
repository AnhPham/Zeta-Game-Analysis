# Hướng Dẫn Setup và Train AI Agent cho Demo Game

## Tổng Quan

Project này sử dụng Unity ML-Agents để train một AI agent có thể chơi game demo. AI sẽ học cách chọn đúng các hình (Cube, Sphere, Capsule) theo thứ tự mà level yêu cầu.

## Yêu Cầu

1. **Unity ML-Agents Package**: Cần cài đặt Unity ML-Agents package vào project
2. **Python Environment**: Cần Python 3.7+ và mlagents package để train
3. **Unity Editor**: Unity 2020.3 LTS trở lên (khuyến nghị)

## Cài Đặt

### Bước 1: Cài Đặt Unity ML-Agents Package

1. Mở Unity Package Manager (Window → Package Manager)
2. Click vào dấu "+" ở góc trên bên trái
3. Chọn "Add package from git URL"
4. Nhập: `com.unity.ml-agents`
5. Hoặc thêm vào `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.unity.ml-agents": "2.0.0"
  }
}
```

### Bước 2: Cài Đặt Python Environment

```bash
# Tạo virtual environment (khuyến nghị)
python -m venv mlagents_env

# Activate virtual environment
# Trên Windows:
mlagents_env\Scripts\activate
# Trên macOS/Linux:
source mlagents_env/bin/activate

# Cài đặt mlagents
pip install mlagents
```

### Bước 3: Setup Scene

1. Mở scene `DemoGame.unity`
2. Tạo một GameObject mới tên là "AIAgent"
3. Add component `DemoAIAgent` vào GameObject này
4. Trong Inspector, assign các references:
   - **Game Controller**: Kéo `DemoGameController` vào
   - **Shapes**: Kéo tất cả 3 `DemoShape` objects (Cube, Sphere, Capsule) vào array
5. Tạo một GameObject mới tên là "AIGameController"
6. Add component `DemoAIGameController` vào
7. Assign references:
   - **Game Controller**: `DemoGameController`
   - **AI Agent**: `AIAgent` GameObject vừa tạo
   - **Use AI**: Bật lên để AI tự động chơi

### Bước 4: Cấu Hình Behavior Parameters

1. Trên GameObject `AIAgent`, Unity sẽ tự động thêm component `Behavior Parameters`
2. Cấu hình:
   - **Behavior Name**: `DemoGameAgent`
   - **Vector Observation**: 
     - **Space Size**: `20` (số lượng observations)
   - **Actions**:
     - **Discrete Actions**: 
       - **Branches Size**: `1`
       - **Branch 0 Size**: `4` (0=Cube, 1=Sphere, 2=Capsule, 3=No-op)

## Training

### Bước 1: Chuẩn Bị Config File

Config file đã được tạo tại: `Assets/ThirdParties/Zeta/ProjectAnalysis/Demo/ML-Agents/demo_game_config.yaml`

Bạn có thể chỉnh sửa các tham số training trong file này:
- `learning_rate`: Tốc độ học (mặc định: 3.0e-4)
- `batch_size`: Kích thước batch (mặc định: 64)
- `buffer_size`: Kích thước buffer (mặc định: 2048)
- `max_steps`: Số bước training tối đa (mặc định: 5,000,000)

### Bước 2: Build và Train

**Cách 1: Train trong Editor (Khuyến nghị cho testing)**

1. Trong Unity Editor, đảm bảo `Behavior Parameters` → **Inference Device** = `CPU`
2. Chạy scene trong Editor
3. Mở terminal và chạy:
```bash
mlagents-learn Assets/ThirdParties/Zeta/ProjectAnalysis/Demo/ML-Agents/demo_game_config.yaml --run-id=demo_game_v1
```

**Cách 2: Build Standalone và Train**

1. Build project thành executable:
   - File → Build Settings
   - Chọn platform (Windows/Mac/Linux)
   - Build
2. Chạy training:
```bash
mlagents-learn Assets/ThirdParties/Zeta/ProjectAnalysis/Demo/ML-Agents/demo_game_config.yaml --run-id=demo_game_v1 --env=<path_to_build>
```

### Bước 3: Monitor Training

Training sẽ tạo ra:
- **TensorBoard logs**: Trong thư mục `results/`
- **Model checkpoints**: Trong thư mục `results/<run-id>/`

Để xem training progress:
```bash
tensorboard --logdir results
```

Mở browser và truy cập: `http://localhost:6006`

## Sử Dụng Trained Model

### Bước 1: Load Model vào Unity

1. Sau khi training xong, model sẽ được lưu tại: `results/<run-id>/<run-id>.onnx`
2. Copy file `.onnx` vào `Assets/ThirdParties/Zeta/ProjectAnalysis/Demo/ML-Agents/Models/`
3. Trong Unity, chọn GameObject `AIAgent`
4. Trong `Behavior Parameters`:
   - **Model**: Kéo file `.onnx` vào
   - **Inference Device**: Chọn `CPU` hoặc `GPU` (nếu có)
5. Đảm bảo `DemoAIGameController` → **Use AI** được bật

### Bước 2: Test AI

1. Chạy scene trong Editor
2. AI sẽ tự động chơi game
3. Quan sát performance của AI

## Tùy Chỉnh Training

### Điều Chỉnh Rewards

Trong `DemoAIAgent.cs`, bạn có thể điều chỉnh rewards:

```csharp
// Phần thưởng khi chọn đúng
AddReward(1.0f);

// Phần thưởng khi hoàn thành level
AddReward(10.0f);

// Phạt khi chọn sai
AddReward(-2.0f);
```

### Điều Chỉnh Observations

Trong method `CollectObservations()`, bạn có thể thêm/bớt observations để cải thiện performance.

### Behavioral Cloning (Optional)

Nếu bạn có demo data từ người chơi thật, có thể sử dụng behavioral cloning:

1. Record demo bằng cách chơi game và ghi lại actions
2. Lưu demo file vào `./demos/DemoGameAgent.demo`
3. Config đã có sẵn behavioral cloning settings

## Troubleshooting

### Lỗi: "ML-Agents package not found"
- Đảm bảo đã cài đặt Unity ML-Agents package
- Kiểm tra `Packages/manifest.json`

### Lỗi: "Behavior name mismatch"
- Đảm bảo `Behavior Parameters` → **Behavior Name** khớp với tên trong config file

### AI không học được
- Kiểm tra rewards có được tính đúng không
- Tăng `learning_rate` hoặc `batch_size`
- Kiểm tra observations có đầy đủ thông tin không

### Training quá chậm
- Giảm `max_steps` để test nhanh
- Sử dụng GPU nếu có
- Giảm số lượng observations nếu không cần thiết

## Tài Liệu Tham Khảo

- [Unity ML-Agents Documentation](https://github.com/Unity-Technologies/ml-agents)
- [ML-Agents Training Guide](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-ML-Agents.md)
- [PPO Algorithm](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-PPO.md)

## Notes

- Training có thể mất vài giờ đến vài ngày tùy vào độ phức tạp
- Khuyến nghị train ít nhất 1-2 triệu steps để có kết quả tốt
- Có thể cần điều chỉnh hyperparameters nhiều lần để tối ưu performance
