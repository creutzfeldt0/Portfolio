using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using ExcelDataReader;
using System.Reflection;

public class SO_Data_Maker
{
    private static SO_Data_Maker       m_Instance;
    private static string m_SaveFolderPath = "Assets/Data/Addressable/ScriptableObject";

    [MenuItem("Assets/CustomAction/XlsxToData")]
    private static void BuildData()
    {
        if( null == m_Instance )
        {
            m_Instance = new SO_Data_Maker();
        }

        m_Instance.ReadExcel();
    }

    private void ReadExcel()
    {
        UnityEngine.Object[] selectedObjects = Selection.objects;

        if( null == selectedObjects )   return;

        Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );

        string path;
        string fullpath;
        string ext;

        // 선택된 오브젝트 안에서 엑셀파일들을 읽어냄 //
        for (int index = 0; index < selectedObjects.Length; index++)
        {
            if( null == selectedObjects[index] )   continue;
            
            path = AssetDatabase.GetAssetPath( selectedObjects[index] );
            ext = Path.GetExtension(path).ToLower();

            if( ext != ".xlsx" ) continue;

            fullpath = Path.GetFullPath( path );

            bool success = false;

            using (var stream = File.Open(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                // 모든 시트를 DataSet으로 로드
                var result = reader.AsDataSet();
                
                foreach (DataTable table in result.Tables)
                {
                    //시트 이름이 맞아야 한다.
                    if( selectedObjects[index].name != table.TableName) continue;

                    success = MakeData( table );
                }
            }

            string clr = "<color=#00ff00>", ret = "Success";
            if (false == success)
            {
                clr = "<color=#ff0000>";
                ret = "Failed";
            }

            // 실패 시트 이름 확인..
            Debug.Log($"[SO_Data_Maker] ReadExcel, {path} ===> {clr}{ret}</color>");
        }
    }

    /// <summary>
    /// 스크립터블 오브젝트 생성 ///
    /// </summary>
    /// <param name="table"></param>
    private bool MakeData( DataTable table )
    {
        if (null == table) return false;

        string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, m_SaveFolderPath);
        if (false == Directory.Exists(fullPath)) //유니티 경로.
        {
            Directory.CreateDirectory(fullPath);
        }

        string savePath = $"{m_SaveFolderPath}/SO_{table.TableName}.asset";
        savePath = savePath.Replace("//", "/");

        Type classType = SearchTypeToString($"{table.TableName}Data");

        if (null == classType) return false;

        Type sobjType = SearchTypeToString( $"SO_{table.TableName}" );

        if (null == sobjType)   return false;

        //SObj 생성..
        var asset = ScriptableObject.CreateInstance(sobjType);

        if (null == asset)      return false;

        //템플릿 함수의 템플릿 타입을 classType 타입으로 지정한다..
        //함수 실행은 아래 포문에서..
        MethodInfo methodInfo = typeof(SO_Data_Maker).GetMethod("AddListToData");

        methodInfo = methodInfo.MakeGenericMethod(classType);

        //엑셀파일 첫번째 줄은 설명을 위한 줄//
        if( 2 > table.Rows.Count )  return false;

        //두번째 줄은 변수명//
        FieldInfo fieldInfo;
        List<FieldInfo> fieldInfos = new List<FieldInfo>();
        string stringData = "";
        string[] stringForSplit;

        //변수명으로 변수 타입 리스트 생성//
        for (int j = 0; j < table.Columns.Count; j++)
        {
            stringData = table.Rows[1][j]?.ToString();

            if(true == string.IsNullOrEmpty(stringData))    
            {
                fieldInfos.Add(null);
                continue;
            }

            //클래스의 변수명으로 찾아 타입을 저장한다.
            fieldInfo = classType.GetField( stringData );

            fieldInfos.Add(fieldInfo);
        }

        //세번째 줄 부터 데이터//
        for (int i = 2; i < table.Rows.Count; ++i)
        {
            //저장데이터 클래스 인스턴스 생성..
            var instance = Activator.CreateInstance(classType);

            //저장데이터 리스트에 미리 추가..
            methodInfo.Invoke(this, new object[] { sobjType, asset, instance } );

            for (int j = 0; j < fieldInfos.Count; j++)
            {
                if( null == fieldInfos[j] )         continue;

                stringData = table.Rows[i][j]?.ToString();

                if(true == string.IsNullOrEmpty(stringData))        continue;

                //변수의 타입에 따라 데이터 파싱..
                switch (fieldInfos[j].FieldType)
                {
                    case Type t when t == typeof(int):
                        if (int.TryParse(stringData, out int n))
                            fieldInfos[j].SetValue(instance, n);
                        break;

                    case Type t when t == typeof(byte):
                        if (byte.TryParse(stringData, out byte bt))
                            fieldInfos[j].SetValue(instance, bt);
                        break;

                    case Type t when t == typeof(string):
                        fieldInfos[j].SetValue(instance, stringData);
                        break;

                    case Type t when t == typeof(float):
                        if (float.TryParse(stringData, out float f))
                            fieldInfos[j].SetValue(instance, f);
                        break;

                    case Type t when t == typeof(bool):
                        if (bool.TryParse(stringData, out bool b))
                            fieldInfos[j].SetValue(instance, b);
                        break;

                    case Type t when t.IsEnum:
                        if (Enum.TryParse(t, stringData, out object enumValue))
                            fieldInfos[j].SetValue(instance, enumValue);
                        break;

                    case Type t when t == typeof(Vector3):
                        string[]tok = stringData.Split(',');
                        if (null != tok && 2 < tok.Length)
                        {
                            float v0 = 0f, v1 = 0f, v2 = 0f;
                            float.TryParse(tok[0], out v0);
                            float.TryParse(tok[1], out v1);
                            float.TryParse(tok[2], out v2);
                            fieldInfos[j].SetValue(instance, new Vector3(v0, v1, v2));
                        }
                        break;

                    // List 처리.
                    case Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>):
                        {
                            Type elementType = t.GetGenericArguments()[0];
                            var list = fieldInfos[j].GetValue(instance);
                            var addMethod = t.GetMethod("Add");
                            stringForSplit = stringData.Split(';');

                            foreach (var s in stringForSplit)
                            {
                                if (elementType == typeof(int) && int.TryParse(s, out int intVal))
                                    addMethod.Invoke(list, new object[] { intVal });
                                else if (elementType == typeof(byte) && byte.TryParse(s, out byte byteVal))
                                    addMethod.Invoke(list, new object[] { byteVal });
                                else if (elementType == typeof(float) && float.TryParse(s, out float floatVal))
                                    addMethod.Invoke(list, new object[] { floatVal });
                                else if (elementType == typeof(string))
                                    addMethod.Invoke(list, new object[] { s });
                                else if (elementType.IsEnum && Enum.TryParse(elementType, s, out object enumVal))
                                    addMethod.Invoke(list, new object[] { enumVal });
                            }
                            break;
                        }
                } //switch
            } //for j
        }

        try
        {
            AssetDatabase.CreateAsset(asset, savePath); //유니티 경로 "Assets/~"를 쓴다.
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SO_Data_Maker] MakeData() Exception: {ex}");
        }

        return true;
    }

    private static Type SearchTypeToString( string typeName )
    {
        //클래스 이름으로 클래스 가져오기..
        Type type = Type.GetType( typeName );

        if (null == type)
        {
            // 프로젝트에 분명히 포함된 클래스임에도 불구하고 Type이 찾아지지 않는다면,
            // 실행중인 어셈블리를 모두 탐색 하면서 그 안에 찾고자 하는 Type이 있는지 검사.
            var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();

            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                var assembly = System.Reflection.Assembly.Load(assemblyName);

                if (assembly == null) continue;

                // 찾았다 요놈!!!
                type = assembly.GetType( typeName );

                if (type != null)
                {
                    return type;
                }
            }
        }

        return type;
    }

    public void AddListToData<T>( Type sobjType, ScriptableObject sObj, T instance )
    {
        FieldInfo fieldInfo = sobjType.GetField("Datas");

        List<T> list = (List<T>)fieldInfo.GetValue(sObj);

        list.Add( instance );
    }
}