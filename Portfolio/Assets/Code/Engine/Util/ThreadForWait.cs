using System;
using System.Threading;

public class ThreadForWait : UnityEngine.CustomYieldInstruction
{
    private bool isRunning;

    public ThreadForWait(Action task, ThreadPriority priority = ThreadPriority.Normal)
    {
        isRunning = true;

        Thread thread = new Thread( () =>
        {
            task();
            isRunning = false;
        } );

        thread.Start( priority );
    }
    public override bool keepWaiting 
    { 
        get { return isRunning; } 
    }
}