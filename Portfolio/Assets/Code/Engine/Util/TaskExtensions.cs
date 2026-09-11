using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public static class TaskExtensions
{
    /// <summary>
    /// List<Task>의 모든 작업이 완료될 때까지 대기하며, 개별 작업이 끝날 때마다 진행도를 콜백합니다.
    /// </summary>
    /// <param name="tasks">추적할 오리지널 Task 리스트</param>
    /// <param name="onProgressChanged">진행도 변경 시 호출될 콜백 (0.0f ~ 1.0f)</param>
    public static async Task WhenAllWithProgress(this List<Task> tasks, Action<float> onProgressChanged)
    {
        if (tasks == null || tasks.Count == 0)
        {
            onProgressChanged?.Invoke(1f); // 작업이 없으면 즉시 100% 처리
            return;
        }

        int totalTaskCount = tasks.Count;
        int completedTaskCount = 0;

        // 진행도를 추적하기 위해 원본 Task들을 감싸줄 래퍼 리스트
        List<Task> progressTrackingTasks = new List<Task>(totalTaskCount);

        for (int i = 0; i < totalTaskCount; i++)
        {
            // 각 Task가 끝나는 시점을 감시하는 로컬 비동기 함수
            async Task TrackProgress(Task originalTask)
            {
                await originalTask; // 원본 작업 완료 대기
                
                // 멀티스레드 안전하게 완료 카운트 증가
                int currentCompleted = Interlocked.Increment(ref completedTaskCount);
                
                // 진행도 계산 (0.0 ~ 1.0)
                float progressPercent = (float)currentCompleted / totalTaskCount;
                
                // 메인 스레드로 안전하게 콜백 전달
                onProgressChanged?.Invoke(progressPercent);
            }

            progressTrackingTasks.Add(TrackProgress(tasks[i]));
        }

        // 감시용 래퍼 Task들이 모두 끝날 때까지 대기
        await Task.WhenAll(progressTrackingTasks);
    }
}