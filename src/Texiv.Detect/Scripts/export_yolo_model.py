from ultralytics import YOLO

def export_yolo_models():
    """
    YOLO 모델을 ONNX 형식으로 내보내는 스크립트
    """
    models = [
        'yolo11n.pt',  # YOLOv11 nano (권장)
        # 'yolo11s.pt',  # YOLOv11 small
        # 'yolov8n.pt',  # YOLOv8 nano (대안)
    ]
    
    for model_name in models:
        print(f"\n{'='*50}")
        print(f"처리 중: {model_name}")
        print('='*50)
        
        try:
            # 모델 로드 (자동 다운로드)
            model = YOLO(model_name)
            print(f"? 모델 로드 완료: {model_name}")
            
            # ONNX로 내보내기
            export_path = model.export(format='onnx')
            print(f"? ONNX 내보내기 완료!")
            print(f"  파일 위치: {export_path}")
            
        except Exception as e:
            print(f"? 오류 발생: {e}")

if __name__ == "__main__":
    print("YOLO 모델 ONNX 변환 도구")
    print("이 스크립트는 YOLO 모델을 다운로드하고 ONNX 형식으로 변환합니다.\n")
    
    export_yolo_models()
    
    print("\n" + "="*50)
    print("완료! 생성된 .onnx 파일을 Texiv.Detect 애플리케이션 폴더로 복사하세요.")
    print("권장 위치: 실행파일/Models/ 폴더")
    print("="*50)
