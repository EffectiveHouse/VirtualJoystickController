<h1>Eduron Controller Application 설명서<h6>Xamarin Android Appliction</h6>

MainActivity Class
==================

---

-	어플리케이션 시작 클래스
-	어플리케이션 기초 셋팅 및 블루투스 연결 체크

Activity 설정<h6> 어플리케이션 이름 , 로고이미지, 화면테마, 화면가로세로 설정
-----------------------------------------------------------------------------

```cs
[Activity(Label = "EDURON", Name = "controller.MainActivity", MainLauncher = true,
          ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize,
          Theme = "@style/Theme.AppCompat.Light.NoActionBar", ScreenOrientation = ScreenOrientation.Landscape, Icon = "@drawable/Logo")]      
```

-	이름: Eduron
-	테마: NoActionBar
-	화면: 가로모드
-	로고: Logo.png(Drogen Img)

Splash <h6>Application 실행 시 인트로 화면 역활
-----------------------------------------------

```cs
	config = new EasySplashScreen(this)
	    .WithFullScreen()
	    .WithTargetActivity(Java.Lang.Class.FromType(typeof(controller.ControllerActivity)))
	    .WithSplashTimeOut(1500) //1.5sec
```

-	Splash 적용(Component: EasySplashScreen)
-	1.5초 동안 위와 같이 설정한 텍스트와 이미지 출력 후 **ControllerActivity 로 화면전환**

ControllerActivity Class
========================

---

-	**8채널 데이터 (T, A, E, R, Aux1, Aux2, Aux3, Aux4) 처리**
-	메인 UI와 axml 연결해 주는 클래스
-	axml의 엘리먼트를 이벤트 등록 및 제어
-	터치 이벤트 처리
-	조이스틱 이벤트 처리

OnGenericMotionEvent
--------------------

```cs
override bool OnGenericMotionEvent(MotionEvent e)
```

<h6> Joystick이 조작이 일어날 때마다 일어는 이벤트 함수 </h6>
<h6> 스틱을 조작 할 때마다 조이패드에 위치한 Stick의 X, Y의 좌표를 갱신하며, 시동이 걸려 있을 시에만 화면에 stick을 그린다.</h6>

| Parameters | Explanation               |
|:----------:|:--------------------------|
|     e      | Joystick의 작동 정보 객체 |

OnKeyDown
---------

```cs
override bool OnKeyDown([GeneratedEnum] Keycod keycode, KeyEvent e)
```

<h6> Joystick이 버튼을 누를 때 마다 발생하는 이벤트 함수</h6>
<h6> 버튼을 눌럿을 때 데이터는 1, 뗐을 때 데이터는 0으로 정의</h6>

| Parameters | Explanation                                    |
|:----------:|:-----------------------------------------------|
|  keycode   | Joystick에 누르는 버튼 종류                    |
|     e      | 이벤트를 발생 시키는 디바이스 정보를 가진 객체 |

OnKeyDown
---------

```cs
override bool OnKeyUp([GeneratedEnum] Keycod keycode, KeyEvent e)
```

<h6> Joystick이 버튼을 뗄 때 마다 발생하는 이벤트 함수</h6>
<h6> 버튼을 눌럿을 때 데이터는 1, 뗐을 때 데이터는 0으로 정의</h6>

| Parameters | Explanation                                    |
|:----------:|:-----------------------------------------------|
|  keycode   | Joystick에 누르는 버튼 종류                    |
|     e      | 이벤트를 발생 시키는 디바이스 정보를 가진 객체 |

OnTouchListenerAnonymousInner Class
===================================

---

-	터치 이벤트 리스너 등록 클래스

OnTouchListenerAnonymousInner 생성자
------------------------------------

```cs
OnTouchListenerAnonymousInnerClass(ControllerActivity outerInstance, VirtualController virtualController)
```

<h6> ControllerActivity에서 일어나는 터치 이벤트를 조이패드에 반영

| Parameters | Explanation                                                              |
|:----------:|:-------------------------------------------------------------------------|
| outerInstance  | 이벤트가 일어나는 메인 UI 객체 |
|virtualController   | 이벤트가 일어났을때 반응되는 객체  |

## OnTouch

```cs
bool OnTouch(View v, MotionEvent e)
```

| Parameters | Explanation                                                              |
|:----------:|:-------------------------------------------------------------------------|
|v   | View 정보 객체  |
|e   | 터치 이벤트(Down, Move, Up) 일어나는 정보 및 터치한 X,Y 좌표 정보를 가지고 있는 객체  |


<h6>터치가 일어날 때마다 일어나는 이벤트 함수</h6>

VirtualController Class
=======================

---

-	메인 UI의 조종패드에 터치 or 외부디바이스 조종에 따른 Stick을 Draw
-	메인 UI의 조종패드에 영역 설정 및 Draw
-	메인 UI의 Stick을 Draw 할 수 있는 영역 Min, Max 및 스틱이 점프하는 범위 설정
	-	조이패드 사이즈에 따라 그릴수 있는 X, Y 좌표가 유동적, 사이즈(650 * 650)일 경우 MIN: 105, MAX: 545
	-	과격한 스틱 점프 방지를 위해 점프할수 있는 최대값 50으로 제한

VirtualController 생성자
------------------------

```cs
VirtualController(Context context, ViewGroup layout, int stickResID)
```

<h6> UI 화면 조종패드와 연동, 스틱 이미지 연동 </h6>

| Parameters | Explanation                                                              |
|:----------:|:-------------------------------------------------------------------------|
|  context   | 실행하고있는 현재 App Context 정보 (스틱과 조이패드를 그리려면 필요하다) |
|   layout   | Axml과 연동된 변수로 Resource 안에 RelativeLayout ID를 가지고 있다.      |
| stickResID | Stick을 표현하려는 이미지 아이디                                         |

DrawStick
---------

```cs
void DrawStick(MotionEvent arg1)
```

<h6>터치되는 X, Y좌표 저장 및 스틱을 그리는 메소드</h6>

| Parameters | Explanation                                                                          |
|:----------:|:-------------------------------------------------------------------------------------|
|    arg1    | 터치 이벤트(Down, Move, Up) 일어나는 정보 및 터치한 X,Y 좌표 정보를 가지고 있는 객체 |

-	Touch 상태 3가지
	-	Touch Down: 화면을 누를 때 발생
	-	Touch Move: 화면을 움직일 때 발생
	-	touch Up: 화면에서 손을 뗐을때 발생

```cs
if (arg1.Action == MotionEventActions.Down)
{
  //점프방지
    drawRenamed.Position(positionX, positionY);
    Draw();
    touchState = true;
}
```

-	Touch Down: 화면을 누를 때 발생
-	점프방지
-	터치 된 지점의 X, Y좌표 저장
-	스틱을 메인UI에 Draw

```cs
else if (arg1.Action == MotionEventActions.Move && touchState)
{
    drawRenamed.Position(positionX, positionY);
    Draw();
}
```

-	Touch Move: 화면을 움직일 때 발생
-	Touch Down인 상태에서 움직일 때만 일어난다.
-	터치 된 지점의 X, Y좌표 저장
-	스틱을 메인UI에 Draw

```cs
else if (arg1.Action == MotionEventActions.Up)
{
    touchState = false;
}
```

-	touch Up: 화면에서 손을 뗐을때 발생

JoystickController Class
========================

---

-	연결 되는 외부 디바이스는 디바이스ID로 관리
-	연결 된 외부 디바이스가 조이스틱 or 게임패드 검증 후 디바이스ID 저장
-	조이스틱 Min, Max 설정(Default: -1 ~ 1) -> (Set: -0.8 ~ 0.8)
-	조이스틱 데이터를 UI좌표로 변환

DualShock Joystick Controller 조이스틱 중요 키맵<h6>왼쪽 패드 값: X,Y 오른쪽 패드 값: RX, RY (**각 X,Y축 범위는 -1 ~ 1**\)
--------------------------------------------------------------------------------------------------------------------------

```cs
//왼쪽 조종 스틱
public static Axis AXIS_X           = MotionEvent.AxisFromString("AXIS_X");
public static Axis AXIS_Y           = MotionEvent.AxisFromString("AXIS_Y");
//오른쪽 조종 스틱
public static Axis AXIS_RX          = MotionEvent.AxisFromString("AXIS_RX");
public static Axis AXIS_RY          = MotionEvent.AxisFromString("AXIS_RY");
//4개 버튼
public static Keycode BUTTON_A      = Keycode.ButtonB;
public static Keycode BUTTON_B      = Keycode.ButtonC;
public static Keycode BUTTON_C      = Keycode.ButtonY;
public static Keycode BUTTON_X      = Keycode.ButtonA;
```

JoystickController 생성자
-------------------------

```cs
JoystickController(VirtualController LeftPad, VirtualController RightPad)
```

<h6>좌, 우 패드의 각 X, Y축 데이터 저장을 위해 입력</h6>

| Parameters | Explanation                                                |
|:----------:|:-----------------------------------------------------------|
|  LeftPad   | ControllerActivity Class에서 작동되고 있는 왼쪽패드 객체   |
|  RightPad  | ControllerActivity Class에서 작동되고 있는 오른쪽패드 객체 |

CheckGameControllers
--------------------

```cs
void CheckGameControllers(int[] getDevicesID)
```

<h6> InputManager로 저장된 Device List 중에 Joystick or Gamepad 체크 후 디바이스ID 등록</h6>

| Parameters   | Explanation                               |
|:------------:|:------------------------------------------|
| getDevicesID | InputManager에 저장되 있는 Device ID 목록 |

### KeyMapping

```cs
void KeyMapping(MotionEvent e)
```

<h6> 조이스틱 LeftStick RightStick 각 X, Y축 데이터의 범위(-1 ~ 1)를 Draw 가능한 좌표로 변환 </h6>
<h6> 조이패드 크기에 따라 Min, Max 값 결정(Ex 사이즈(650 * 650)경우 MIN: 105, MAX: 545 )</h6>

| Parameters | Explanation                                    |
|:----------:|:-----------------------------------------------|
|     e      | Joystick의 현재 X, Y축 정보를 가지고 있는 객체 |

IsGamepad
---------

```cs
 IsGamepad(InputDevice device)
```

<h6>외부장치 device가 Joystick or Gamepad 체크 후 Ture of False 리턴</h6>

| Parameters | Explanation                                |
|:----------:|:-------------------------------------------|
|   device   | 안드로이드 시스템과 연결 된 외부 장치 객체 |
