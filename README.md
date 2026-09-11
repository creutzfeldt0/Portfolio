# Portfolio

![Unity](https://img.shields.io/badge/unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white) 6000.6.0f1 버전 이용.

프로젝트에서 직접 작성한 코드들 모음  
  
1. NGUI 기본 UI 구조  
    * Assets\Code\Engine\UI\NGUI\Scene_Base.cs, Popup_Base.cs  
        - 씬에서 팝업들을 관리하는 형태.  
        - 씬에서 옵션 변화를 감지하여 LoaderBase.cs를 상속 받은 Label, Texture, SpriteLoader에 국가에 맞는 폰트, 텍스트, 이미지 등을 적용함.  
    * LabelLoader.cs  
        - 그 중 Label 예시 코드  

2. 애셋번들 리소스 관리 시스템  
    * 코루틴 이용.  
    * Assets\Code\Engine\Manager\AssetBundle\AssetBundleMgr.cs  
        - 애셋번들 생성 시 버전에 맞는 패치 카탈로그 제작.
        - Resources 안 특정 폴더의 리소스들을 옮겨 애셋번들로 만들고 빌드한 다음 다시 원상복귀 시키는 코드 방식.  
    * Assets\Code\Engine\Manager\AssetBundle\ResourceMgr.cs
        - 첫 로딩화면에서 카탈로그를 확인하고 패치용량을 체크, 패치 진행.
        - 한번에 번들을 로드하여 필요시 리소스를 가져오는 구조.

3. 어드레서블 리소스 로딩 시스템
    * Task, Async 이용.
    * Assets\Code\Engine\Manager\Addressable\ResourceManager.cs
        - 카탈로그에 맞춰 다운로드 용량 체크하는 함수 제작.
        - 처음에 로딩하여 끝까지 들고있을 애셋과 필요시 불렀다 지워줄 애셋을 분리해 처리.        

4. 엑셀 테이블을 ScriptableObject로 변경하는 툴
    * Assets\Data\None\DataTable\Achievement.xlsx
        - 유니티 Project 창에서 해당 엑셀 파일 우클릭.
        - 메뉴에서 CustomAction -> XlsxToData 클릭.
    * Assets\Data\Addressable\ScriptableObject\SO_Achievement.asset
        - 파일이 생성되는 것을 확인 할 수 있다.  
    * Assets\Code\Game\Data\AchievementData.cs
        - 해당 엑셀파일의 이름과 같은 시트를 찾고 같은 이름의 Data 클래스를 찾은 다음.
        - 클래스에 맞는 변수들을 엑셀파일에서 찾아 그 데이터로 ScriptableObject를 생성한다.
    * Assets\Code\Engine\Editor\ScriptableObject\SO_Data_Maker.cs
        - 해당 파일 참고.

5. 스파인 FX 툴 
    * UIToolkit으로 제작. 
    * FX디자이너가 스파인 파일을 가져와서 화면에서 애니메이션을 시간별로 보면서 이펙트를 적용가능.
    * 하단 영상에 나온 연출을 제작하기 위해 사용.

    <table>
        <tr>
            <td width="300">
                <video src="https://github.com/user-attachments/assets/d91af52d-7b96-4f6c-b202-79cce5c26f5a" width="100%"></video>
            </td>
            <td width="1000">
                <img src="./Portfolio/Assets/SpineToolResultVideo/FX 스파인툴.png" width="100%" />
            </td>
        </tr>
    </table>

    * Assets\Code\Engine\UI\UIToolkit\
        - UIToolkit 관련 UI 코드.
    * Assets\Data\Addressable\Scenes\Scene_SpineTool.unity
        - 해당 씬.
    * Assets\Data\None\SpineFXEditor\
        - 해당 리소스.

