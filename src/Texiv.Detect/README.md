# TEXIV Garment Detect - YOLO 의류 추적 시스템

## 개요
WPF 기반 데스크톱 애플리케이션으로, 최신 YOLO (YOLOv8/YOLOv11) 모델을 사용하여 비디오에서 의류 및 사람을 실시간으로 탐지하고 추적합니다.

## 주요 기능
- ? **최신 YOLO 지원**: YOLOv11, YOLOv8 ONNX 모델 사용
- ? **실시간 탐지**: 비디오에서 실시간으로 객체 탐지
- ? **시각적 피드백**: 바운딩 박스와 신뢰도 점수 표시
- ? **의류 필터링**: 의류 관련 클래스만 선별적으로 표시
- ? **다양한 비디오 형식**: MP4, AVI, MKV, MOV 등 지원
- ? **사용자 친화적**: 직관적인 GUI 인터페이스

## 시스템 요구사항
- Windows 10/11 (64-bit)
- .NET 10.0 Runtime
- 최소 4GB RAM (8GB 권장)
- CPU: Intel Core i5 이상 또는 동급 AMD
- 디스크 공간: 최소 100MB

## 빠른 시작 가이드

### 1단계: 프로젝트 빌드
```bash
# 프로젝트 디렉토리로 이동
cd src/Texiv.Detect

# 빌드
dotnet build

# 또는 릴리스 빌드
dotnet build -c Release
```

### 2단계: YOLO 모델 준비
YOLO 모델이 필요합니다. 두 가지 방법이 있습니다:

#### 방법 A: Python 스크립트 사용 (권장)
```bash
# Python 및 Ultralytics 설치
pip install ultralytics

# 제공된 스크립트 실행
python Scripts/export_yolo_model.py
```

#### 방법 B: 수동 다운로드
```bash
# Ultralytics CLI 사용
pip install ultralytics
yolo export model=yolo11n.pt format=onnx
```

자세한 내용은 [YOLO_MODEL_SETUP.md](YOLO_MODEL_SETUP.md) 참조

### 3단계: 모델 배치
생성된 `yolo11n.onnx` 파일을 다음 위치에 배치:
```
bin/Debug/net10.0-windows10.0.19041/Models/yolo11n.onnx
```

또는 실행 시 "Load YOLO Model" 메뉴로 직접 선택

### 4단계: 애플리케이션 실행
```bash
dotnet run
```

또는 빌드된 실행 파일을 직접 실행

## 사용 방법

### 기본 워크플로우
1. **모델 로드**
   - 메뉴: `File → Load YOLO Model`
   - `.onnx` 파일 선택
   - 상태 표시줄에서 "Model: Loaded ?" 확인

2. **비디오 열기**
   - 메뉴: `File → Open Video`
   - 분석할 비디오 파일 선택

3. **탐지 시작**
   - `▶ Play` 버튼 클릭
   - 실시간으로 탐지 결과 확인
   - `? Pause`로 일시 정지

### 단축키
- `Ctrl+O`: 비디오 열기
- `Ctrl+L`: YOLO 모델 로드
- `Space`: 재생/일시정지 (곧 추가 예정)

## 탐지 가능한 객체

### COCO 데이터셋 기반 클래스
현재 애플리케이션은 다음 의류 관련 클래스를 강조 표시합니다:
- ?? **Person** (사람)
- ?? **Backpack** (백팩)
- ?? **Umbrella** (우산)
- ?? **Handbag** (핸드백)
- ?? **Tie** (넥타이)
- ?? **Suitcase** (여행 가방)

> **참고**: COCO 데이터셋은 일반 객체 탐지용입니다. 더 정확한 의류 탐지를 위해서는 Fashion 데이터셋으로 fine-tuning된 모델을 사용하세요.

### 바운딩 박스 색상
각 객체 유형은 구별을 위해 다른 색상으로 표시됩니다:
- ?? **Person**: 주황색
- ?? **Backpack**: 파란색
- ?? **Handbag**: 초록색
- ?? **Umbrella**: 노란색
- ?? **기타**: 빨간색

## 프로젝트 구조
```
src/Texiv.Detect/
├── Views/
│   ├── MainWindow.xaml          # UI 레이아웃
│   └── MainWindow.xaml.cs       # UI 로직
├── ViewModels/
│   └── MainViewModel.cs         # MVVM 패턴 ViewModel
├── Services/
│   ├── YoloDetectionService.cs  # YOLO 추론 로직
│   └── VideoProcessingService.cs # 비디오 처리 로직
├── Converters/
│   └── ValueConverters.cs       # XAML 값 변환기
├── Models/                      # YOLO 모델 저장 폴더
└── Texiv.Detect.csproj          # 프로젝트 파일
```

## 기술 스택

### 프레임워크 & 라이브러리
- **.NET 10** - 최신 .NET 플랫폼
- **WPF** - Windows Presentation Foundation
- **ReactiveUI 22.3.1** - MVVM 프레임워크
- **Microsoft.ML.OnnxRuntime 1.20.1** - ONNX 추론 엔진
- **OpenCvSharp4 4.10.0** - OpenCV wrapper for .NET
- **SixLabors.ImageSharp 3.1.5** - 이미지 처리

### AI/ML
- **YOLO (v8/v11)** - Ultralytics YOLO
- **ONNX** - Open Neural Network Exchange

## 성능 최적화

### 권장 설정
- **모델**: `yolo11n.onnx` (nano 버전, 가장 빠름)
- **해상도**: 640x640 (YOLO 기본값)
- **신뢰도 임계값**: 0.25 (기본값)
- **IoU 임계값**: 0.45 (기본값)

### 성능 향상 팁
1. **가벼운 모델 사용**: nano < small < medium
2. **비디오 해상도 조정**: 고해상도는 처리 시간 증가
3. **GPU 가속**: ONNX Runtime GPU 버전 사용 (선택사항)
4. **프레임 스킵**: FPS를 줄여 처리 속도 향상

## 문제 해결

### "No model found" 오류
- ONNX 모델 파일이 `Models/` 폴더 또는 실행 파일과 같은 디렉토리에 있는지 확인
- "Load Model" 메뉴를 사용하여 수동으로 모델 선택

### 비디오 재생 안 됨
- 지원되는 코덱인지 확인 (H.264, H.265 권장)
- K-Lite Codec Pack 설치 고려

### 탐지 성능이 느림
- 더 작은 모델 사용 (nano 버전)
- 비디오 해상도 줄이기
- 더 강력한 CPU 사용 또는 GPU 가속 활성화

### 빌드 오류
```bash
# NuGet 패키지 복원
dotnet restore

# 클린 후 재빌드
dotnet clean
dotnet build
```

## 향후 계획 (Roadmap)
- [ ] GPU 가속 지원 (CUDA)
- [ ] 커스텀 의류 모델 학습 기능
- [ ] 실시간 카메라 입력 지원
- [ ] 탐지 결과 통계 및 내보내기
- [ ] 다중 객체 추적 (Multi-Object Tracking)
- [ ] 설정 UI (신뢰도 임계값 등)

## 기여하기
기여를 환영합니다! Pull Request를 보내주세요.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 라이선스
이 프로젝트는 관련 오픈소스 라이선스를 준수합니다:
- YOLO: AGPL-3.0 License (Ultralytics)
- OpenCV: Apache 2.0 License
- .NET: MIT License

## 참고 자료
- [Ultralytics YOLO](https://github.com/ultralytics/ultralytics)
- [ONNX Runtime](https://onnxruntime.ai/)
- [OpenCvSharp](https://github.com/shimat/opencvsharp)
- [ReactiveUI](https://www.reactiveui.net/)

## 지원
이슈나 질문이 있으시면 GitHub Issues를 통해 문의해주세요.

---
**Made with ?? for TEXIV Garment Detection**
