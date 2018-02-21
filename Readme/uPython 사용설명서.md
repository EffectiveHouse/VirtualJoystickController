# MicroPython 사용법

---

## RECEIVER

---

- receiver는 SBUS와 CPPM, Bluetooth입력을 지원
- Bluetooth는 CPPM과 동일한 형태의 자료 구조를 사용
- 8개의 채널만 사용 (SBUS의 경우 8개 이상의 채널이 수신되나, 실제로 사용하는 것은 8개로 제한)

| @ | 1st_L | 1st_H | ... | 8th_L | 8th_H | \r |
|:-:|:-----:|:-----:|:---:|:-----:|:-----:|:--:|

### pyb.Receiver(arg1)

- receiver 객체 생성

```python
receiver = pyb.Receiver()                               # arg1을 생략할 경우 SBUS모드로 동작
receiver = pyb.Receiver(protocol = pyb.Receiver.SBUS)   # SBUS모드
receiver = pyb.Receiver(protocol = pyb.Receiver.CPPM)   # CPPM모드
receiver = pyb.Receiver(protocol = pyb.Receiver.BT)     # Bluetooth 모드
```

### receiver.ready()

- receiver가 Bluetooth모드일 경우에만 사용
- 새로운 RC 패킷이 들어왔는지 검사 (Bluetooth의 경우, disconnect가 빈번할 수도 있어서 사용)

```python
ready = receiver.ready()
if ready == True:
    # do something
else:
    # do something
```

### receiver.keyinput()

- 마지막으로 수신된 8개의 채널을 리턴
- Bluetooth의 경우, 위의 ready()이후에 사용하는 것도 고려
- SBUS/CPPM의 경우 앞 4개 채널(throttle, roll, pitch, yaw)에 대한 채널 순서 확인해야 함
- 아래 예제는 TAER1234인 경우

```python
thr, rol, pit, yaw, aux1, aux2, aux3, aux4 = receiver.keyinput()
```

### receiver.rawdata(arg1)

- 마지막으로 수신된 RC입력값 중, arg1로 선택한 채널의 raw값을 리턴
- 1000 ~ 2000 사이의 값일 것
- arg1의 인덱스는 1 ~ 8사이의 값이어야 함

```python
aux1 = receiver.keyinput(5)
aux2 = receiver.keyinput(6)
```

### receiver.angle(arg1)

- RC입력값에 대해서 기울기 - 각도로 변환된 값을 리턴
- 스틱 최대치의 최대 입력 각도는 -50 ~ 50도 사이
- angle 모드로 비행 시, 기체의 기울기 목표값으로 사용함
- arg1의 인덱스는 1 ~ 8사이의 값이어야 함
- 아래 예는 채널순서가 TAER인 경우

```python
desired_thr = receiver.rawdata(1)   # 보통 throttle은 angle이 아닌, 입력값(1000~2000)을 그대로 사용
desired_rol = receiver.angle(2)     # roll에 대한 리시버 입력값
desired_pit = receiver.angle(3)     # pitch에 대한 리시버 입력값
desired_yaw = receiver.angle(4)     # yaw에 대한 리시버 입력값
```

### receiver.percent(arg1)

- RC입력값에 대한 percent
- RC입력값이 1500일 때 0%, 1000이면 -100%, 2000이면 100%로 표현
- arg1의 인덱스는 1 ~ 8사이의 값이어야 함

```python
desired_thr = receiver.percent(1)
desired_rol = receiver.percent(2)
desired_pit = receiver.percent(3)
desired_yaw = receiver.percent(4)
aux1 = receiver.percent(5)
```

---

## MOTION

---

- motion sensor는 ICM20689를 사용 (3-axis accelerometer, 3-axis gyroscope)
- 초당 1000번 데이터가 갱신되므로 이 이하의 빈도로 처리해야 함
- gyroscope는 초당 -1000도부터 1000도까지 처리
- accelerometer는 초당 -8g부터 8g까지 처리

### pyb.Motion(arg1, arg2, arg3)

- Motion 객체 생성
- arg1, arg2, arg3은 생략 가능
- rotation방향은 진행방향 기준, 시계방향으로 0도, 90도, 180도 270도로 지정(센서가 바닥면에 붙는 경우에 대한 고려는 없음)
- arg1 : rotation (pyb.Motion.CW0 / pyb.Motion.CW90 / pyb.Motion.CW180 / pyb.Motion.CW270)
- arg2 : gyroscope의 LPF corner frequency
- arg3 : accelerometer의 LPF corner frequency
- 객체 생성 시의 각 LPF corner frequency는 세 축 모두 같은 값으로 일괄적용 (축별로 달리 하기 위해서는 아래에서와 같이 별도로 지정해 줘야 함)

```python
# 기본값으로 생성 (rotation = pyb.Motion.CW0, gyrolpf = 90, accellpf = 90)
motion = pyb.Motion()
# rotation만 지정하고 LPF는 기본으로 생성 (gyrolpf = 90, accellpf = 90)
motion = pyb.Motion(rotation = pyb.Motion.CW0)
# rotation, gyroscope LPF까지 지정하고, accelerometer의 LPF는 기본으로 생성 (accellpf = 90)
motion = pyb.Motion(rotation = pyb.Motion.CW0, gyrolpf = 60)
# 모두 지정
motion = pyb.Motion(rotation = pyb.Motion.CW0, gyrolpf = 60, accellpf = 60)
```

### motion.reset()

- motion 센서의 LPF를 초기화
- 센서 캘리브레이션등을 했을 때 사용

```python
motion.reset()
```

### motion.rotation(arg1)

- motion 센서의 방향을 새로이 지정
- arg1 : rotation (pyb.Motion.CW0 / pyb.Motion.CW90 / pyb.Motion.CW180 / pyb.Motion.CW270)
- arg를 생략할 경우 현재 설정되어 있는 rotation 값을 출력

```python
motion.rotation(rotation = pyb.Motion.CW0)  # motion센서의 방향을 CW0로 새로이 지정
```

### motion.gyrolpf(arg1, arg2, arg3)

- gyroscope 값에 대해서 적용할 low-pass filter의 corner frequency를 지정
- LPF corner frequency에 대한 적절한 range검사가 없으므로 주의해서 적용해야 함
- motion 센서의 rawdata를 취득하여 FFT를 거쳐서 산출하는 것이 제일 좋으나 실험을 통해 적용해도 무방함
- arg1만 주어질 경우, 세 축에 대하여 동일하게 적용
- arg1, arg2, arg3이 주어질 경우, 각 X, Y, Z축에 대하여 별도로 적용
- arg를 생략할 경우 현재 설정되어 있는 LPF corner frequency 값을 각 축별로 출력

```python
motion.gyrolpf(60)          # gyroscope의 LPF corner frequency를 세 축 모두 60Hz로 지정
motion.gyrolpf(60, 70, 80)  # gyroscope의 LPF corner frequency를 각각 60, 70, 80Hz로 지정
```

### motion.accel(arg1, arg2, arg3)

- accelerometer 값에 대해서 적용할 low-pass filter의 corner frequency를 지정
- LPF corner frequency에 대한 적절한 range검사가 없으므로 주의해서 적용해야 함
- motion 센서의 rawdata를 취득하여 FFT를 거쳐서 산출하는 것이 제일 좋으나 실험을 통해 적용해도 무방함
- arg를 생략할 경우 현재 설정되어 있는 LPF corner frequency 값을 각 축별로 출력

```python
motion.accellpf(60)             # accelerometer의 LPF corner frequency를 세 축 모두 60Hz로 지정
motion.accellpf(60, 70, 80)     # accelerometer의 LPF corner frequency를 각각 60, 70, 80Hz로 지정
```

### motion.gyrobias(arg1, arg2, arg3)

- motion 센서가 과거에 calibration되었고, 그 값을 가지고 있을 경우, 별도의 calibration절차 없이 설정하여 사용하도록 함
- arg가 생략될 경우, 현재 설정되어 있는 gyroscope의 각 축별 calibration값을 출력

```python
motion.gyrobias(10, -20, 10)    # X, Y, Z 각 축에 대하여 bias값을 적용
```

### motion.accelbias(arg1, arg2, arg3)

- motion 센서가 과거에 calibration되었고, 그 값을 가지고 있을 경우, 별도의 calibration절차 없이 설정하여 사용하도록 함
- arg가 생략될 경우, 현재 설정되어 있는 accelerometer의 각 축별 calibration값을 출력

```python
motion.accelbias(10, -20, 10)    # X, Y, Z 각 축에 대하여 bias값을 적용
```

### motion.cali(arg1)

- motion센서 최초 사용 시, 또는 bias되지 않은 상태일 경우 calibration을 시도
- arg1값은 bias에 사용할 샘플의 숫자로, 시스템 loop타임을 고려하여 주어져야 함
- 예를 들어 시스템이 500Hz로 동작할 경우 arg1은 최소 500이상의 값이 주어져야 1초 가량 샘플링을 하여 그 값을 bias할 수 있음
- calibration하는 동안에는 안정된 상태를 유지해야 함

```python
motion.cali(500)        # motion센서가 500개의 값을 샘플링하여 bias를 정함
```

### motion.update(arg1, arg2)

- 가장 최근에 업데이트된 motion값을 읽어옴
- arg1 : 직전에 읽어들인 시간과 현재 읽어가는 시간 사이의 차이 - delta time
- arg2 : 필터를 거쳐서 읽어들일 것인지, rawdata를 그대로 읽을 것인지 결정
- gyroscope의 X, Y, Z축에 대한 값과 accelerometer의 X, Y, Z축에 대한 값을 동시에 리턴
- 순서는 gx, gy, gz, ax, ay, az
- gyroscope는 Degree Per Second, accelerometer는 g로 리턴함

```python
# system loop time이 0.02초 - 500Hz일 경우
gx, gy, gz, ax, ay, az = motion.update(0.02, True)      # LPF를 거친 데이터를 읽어들임
gx, gy, gz, ax, ay, az = motion.update(0.02, False)     # raw data를 읽어들임
```

---

## AHRS

---

- 자세 제어를 위한 시스템
- 기본 좌표계는 NWU - North West Up을 사용함
- mahony algorithm과 madgwick algorithm을 선택하여 사용할 수 있음
- 각 알고리즘 별로 gain설정이 다르기에 주의해야 함
- gain이 클 경우 최근의 motion값을 빠르게 따라가고, gain이 작을 경우 최근의 motion값을 느리게 따라감

### pyb.Ahrs(arg1)

- arg1을 지정하지 않을 경우, 기본은 mahony algorithm으로 동작함
- mahony의 경우 두개의 gain을 사용함 (기본값은 Kp = 0.22, Ki = 0)
- madgwick의 경우 한개의 gain을 사용함 (기본값은 beta = 0.05)

```python
ahrs = pyb.Ahrs()                               # mahony 알고리즘 사용
ahrs = pyb.Ahrs(algorithm = pyb.Ahrs.MAHONY)    # mahony 알고리즘 사용
ahrs = pyb.Ahrs(algorithm = pyb.Ahrs.MADGWICK)  # madgwick 알고리즘 사용
```

### ahrs.reset()

- AHRS를 리셋함
- Motion sensor calibration등을 한 후에 사용
- 함부로 reset할 경우, 현재 드론의 자세를 정확하게 추정할 수 없으므로 주의해야 함

```python
ahrs.reset()
```

### ahrs.gain(arg1, arg2)

- AHRS의 gain을 새로 지정함
- algorithm에 따라서 gain의 수가 틀리므로 주의해야 함
- mahony algorithm의 경우, arg2를 생략하면 Ki gain이 0으로 설정됨
- arg를 생략할 경우, 현재 설정된 gain값을 리턴

```python
# MAHONY의 경우
ahrs.gain(0.27)             # Kp를 0.27, Ki를 0으로 설정
ahrs.gain(0.27, 0.001)      # Kp를 0.27, Ki를 0.001로 설정
# MADGWICK의 경우
ahrs.gain(0.03)             # beta를 0.03으로 설정
```

### ahrs.quaternion()

- AHRS의 가장 최근에 계산된 자세에 대한 quaternion을 리턴

```python
q0, q1, q2, q3 = ahrs.quaternion()
```

### ahrs.update(arg1, arg2, arg3, arg4, arg5, arg6, arg7)

- 7개의 arg가 모두 주어져야 함
- 앞의 3개는 gyroscope의 X, Y, Z값이고 뒤따르는 3개는 accelerometer의 X, Y, Z값
- 마지막 arg7은 앞서 업데이트된 시간과 현재 업데이트하고자 하는 시간 간의 차이 - delta time
- 결과는 euler angle의 형태로 degree 단위로 리턴

```python
# system loop time이 0.02초 - 500Hz일 경우
# motion sensor로부터 현재의 motion정보를 읽어옴
gx, gy, gz, ax, ay, az = motion.update(0.02, True)
# motion정보로부터 현재의 자세를 추정함
euler_r, euler_p, euler_y = ahrs.update(gx, gy, gz, ax, ay, az, 0.02)
```

---

## PID

---

- 가장 기본적인 형태의 PID제어기 사용

### pyb.Pid(arg1, arg2, arg3, arg4, arg5)

- I-Term에 Trapezoidal Integration을 사용하는 형태
- D-Term의 급작스런 변화를 막기 위해 LPF를 무조건 사용
- arg1 : Kp (생략시 1.77)
- arg2 : Ki (생략시 2.52)
- arg3 : Kd (생략시 0.0037)
- arg4 : iterm limit (생략시 200)
- arg5 : D-Term LPF corner frequency (생략시 150)

```python
pid = pyb.Pid()                             # 기본값으로 PID객체 생성
pid = pyb.Pid(0.18, 0.1, 0.0)               # Kp, Ki, Kd만 지정하고 나머진 기본값 생성
pid = pyb.Pid(0.18, 0.1, 0.0, 200)          # Kp, Ki, Kd, max iterm까지 지정하고, LPF 는 기본값 사용
pid = pyb.Pid(0.18, 0.1, 0.0, 200, 70)      # 모두 지정
```

### pid.reset()

- PID객체의 내부에 저장된 previous error와 iterm누적분을 삭제
- 그리고 LPF를 reset함
- motion sensor를 calibration한 경우나 disarm된 경우에 사용

```python
pid.reset()
```

### pid.gain(arg1, arg2, arg3)

- Kp, Ki, Kd를 모두 재지정
- 동시에 3개를 모두 지정해 줘야 함
- 생략할 경우 현재 설정된 Kp, Ki, Kd를 리턴

```python
pid.gain(0.18, 0.1, 0.0003)     # Kp, Ki, Kd를 각각 0.18, 0.1, 0.0003으로 지정
```

### pid.maxiterm(arg1)

- 최대 I-Term 값을 새로이 지정
- arg를 생략할 경우, 현재 설정된 max I-Term값을 리턴
- 설정값은 ABS(ITerm)임을 주의

```python
pid.maxiterm(300)       # MAX. ITerm을 -300 ~ 300까지로 제한
```

### pid.dtermlpf(arg1)

- 급격한 D-Term 변화로 인한 충격을 줄이기 위한 LPF의 corner frequency를 새로이 지정
- arg를 생략할 경우, 현재 설정되어 있는 corner frequency를 리턴

```python
pid.dtermlpf(90)        # D-Term의 LPF corner frequency를 90Hz로 지정
```

### pid.update(arg1, arg2, arg3)

- arg1 : setpoint - 원하는 값
- arg2 : input - 측정된 값
- arg3 : dt - 과거 update로부터 현재 update까지의 시간 (delta time)

```python
# system loop time이 0.02초 - 500Hz일 경우
out_rol = pid_rol.update(setpoint_rol, input_rol, 0.02)     # roll에 대한 PID 계산
out_pit = pid_pit.update(setpoint_pit, input_pit, 0.02)     # pitch에 대한 PID 계산
out_yaw = pid_yaw.update(setpoint_yaw, input_yaw, 0.02)     # yaw에 대한 PID 계산
```

---

## MIXER

---

- Quad와 Hexa X 형태의 frame 지원
- 각 모터별 추력 배분을 결정
- 모터의 일련번호와 회전방향은 아래 참고

```
       QUAD               HEXA
    4        1         4        1
      \    /             \    /
        ||            6 ---||--- 5
      /    \             /    \
    3        2         3        2

  1:CW, 2:CCW, 3:CW, 4:CCW, 5:CW, 6:CCW
```

### pyb.Mixer(arg1)

- arg1은 pyb.Mixer.QUAD 또는 pyb.Mixer.HEXA만 사용 가능
- arg를 생략할 경우 QUAD가 기본으로 설정

```python
mixer = pyb.Mixer()                 # QUAD형태로 mixer 객체 생성
mixer = pyb.Mixer(pyb.Mixer.QUAD)   # QUAD형태로 mixer 객체 생성
mixer = pyb.Mixer(pyb.Mixer.HEXA)   # HEXA형태로 mixer 객체 생성
```

### mixer.reset()

- 사용자 정의 mixing weight를 원래의 시스템 기본값으로 변경

```python
mixer.reset()
```

### mixer.motorcount()

- mixer형태에 따라 사용해야 하는 모터의 수를 리턴

```python
n_motor = mixer.motorcount()
```

### mixer.gain(arg1, arg2, arg3, arg4)

- 사용자 정의 mixing value를 지정
- arg1은 모터 번호 (모터번호는 1부터 시작)
- arg2, arg3, arg4는 arg1에서 지정한 모터에 대한 roll, pitch, yaw에 대한 mixing value
- arg2, arg3, arg4를 생략할 경우 arg1에서 지정한 모터 번호에 대한 현재 mixing value를 리턴

```python
mixer.gain(1, -1, 1, 1)     # 1번 모터의 mixing value를 RPY에 대해서 각각 -1, 1, 1로 설정
mixer.gain(2, -1, -1, -1)   # 2번 모터의 mixing value를 RPY에 대해서 각각 -1, -1, -1로 설정
```

## mixer.mixing(arg1, arg2, arg3, arg4)

- arg1로 지정된 모터에 arg2, arg3, arg4로 주어진 RPY값에 대해서 mixing 시도
- arg2, arg3, arg4는 PID제어기를 통해 나온 RPY 출력 조정값
- 이를 각 모터에 대해서 추력을 배분하는 것
- arg1은 1부터 Mixer.motorcount() 사이에 해당하는 번호이어야 함

```python
# QUAD일 경우 총 4번의 mixing이 일어나야 함
mixer.mixing(1, pid_rol, pid_pit, pid_yaw)  # 1번 모터에 대해 mixing시도
mixer.mixing(2, pid_rol, pid_pit, pid_yaw)  # 2번 모터에 대해 mixing시도
mixer.mixing(3, pid_rol, pid_pit, pid_yaw)  # 3번 모터에 대해 mixing시도
mixer.mixing(4, pid_rol, pid_pit, pid_yaw)  # 4번 모터에 대해 mixing시도
```
