# YOLO 모델 다운로드 및 설정 가이드

## 1. Python 및 Ultralytics 설치

### Python 설치 (아직 설치하지 않은 경우)
1. https://www.python.org/downloads/ 에서 Python 다운로드
2. 설치 시 "Add Python to PATH" 체크

### Ultralytics 라이브러리 설치
```bash
pip install ultralytics
```

## 2. YOLO 모델을 ONNX로 내보내기

### 방법 A: 명령줄 사용 (가장 간단)
```bash
# YOLOv11n (nano - 가장 빠름, 권장)
yolo export model=yolo11n.pt format=onnx

# YOLOv11s (small - 균형잡힌)
yolo export model=yolo11s.pt format=onnx

# YOLOv11m (medium - 더 정확)
yolo export model=yolo11m.pt format=onnx

# YOLOv8n (이전 버전, 안정적)
yolo export model=yolov8n.pt format=onnx
```

### 방법 B: Python 스크립트 사용
`export_yolo.py` 파일 생성:
```python
from ultralytics import YOLO

# 모델 로드 (자동으로 다운로드됨)
model = YOLO('yolo11n.pt')

# ONNX로 내보내기
model.export(format='onnx')

print("모델이 성공적으로 내보내졌습니다!")
print(f"파일 위치: {model.export()}")
```

실행:
```bash
python export_yolo.py
```

## 3. 모델 파일 배치

내보낸 `.onnx` 파일을 다음 위치 중 하나에 배치:

### 옵션 A: Models 폴더 (권장)
```
Texiv.Detect.exe와 같은 디렉토리/
├── Texiv.Detect.exe
└── Models/
    └── yolo11n.onnx
```

### 옵션 B: 실행 파일과 같은 디렉토리
```
Texiv.Detect.exe와 같은 디렉토리/
├── Texiv.Detect.exe
└── yolo11n.onnx
```

### 옵션 C: 애플리케이션에서 직접 선택
- 프로그램 실행 후 **File → Load YOLO Model** 메뉴 사용

## 4. 권장 모델

| 모델 | 크기 | 속도 | 정확도 | 용도 |
|------|------|------|--------|------|
| yolo11n.onnx | ~6MB | 매우 빠름 | 양호 | **실시간 처리 (권장)** |
| yolo11s.onnx | ~22MB | 빠름 | 좋음 | 균형잡힌 성능 |
| yolo11m.onnx | ~50MB | 보통 | 우수 | 높은 정확도 필요시 |
| yolov8n.onnx | ~6MB | 매우 빠름 | 양호 | 안정적인 이전 버전 |

## 5. 문제 해결

### "ultralytics를 찾을 수 없습니다" 오류
```bash
pip install --upgrade ultralytics
```

### "모델 다운로드 실패" 오류
인터넷 연결을 확인하고 다시 시도하거나, 직접 다운로드:
- https://github.com/ultralytics/assets/releases

### ONNX Runtime 오류
최신 버전으로 업데이트:
```bash
pip install --upgrade onnxruntime
```

## 6. 의류 탐지를 위한 커스텀 모델 (고급)

기본 COCO 데이터셋 모델은 일반 객체를 탐지합니다. 의류에 특화된 모델을 원한다면:

1. **DeepFashion 데이터셋**으로 fine-tuning
2. **Fashion-MNIST** 또는 **Fashion-YOLO** 모델 사용
3. 직접 의류 데이터셋으로 학습

```python
from ultralytics import YOLO

# 커스텀 데이터로 학습
model = YOLO('yolo11n.pt')
model.train(data='fashion.yaml', epochs=100)

# ONNX로 내보내기
model.export(format='onnx')
```

## 7. 테스트 비디오 추천

의류가 포함된 테스트 비디오:
- 패션쇼 영상
- 쇼핑몰 CCTV 영상
- 거리 보행자 영상
- 의류 제품 홍보 영상

## 참고 자료
- Ultralytics 공식 문서: https://docs.ultralytics.com/
- YOLO GitHub: https://github.com/ultralytics/ultralytics
- ONNX 형식 가이드: https://onnx.ai/
