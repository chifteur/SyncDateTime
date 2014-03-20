using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncDateTime.Logging
{
    ///
    /// Asynchronous file appender based on the article by Venu Gopal, but modified by Mario Di Vece
    /// Original article: http://technico.qnownow.com/asynchronous-logging-to-a-file-using-log4net-c/
    ///
    public class AsyncFileAppender : RollingFileAppender
    {



        private Queue<LoggingEvent> EventLoggingQueue;
        private readonly object SyncRoot = new object();
        private readonly ManualResetEvent AppenderShutdownResetEvent;
        private bool ShutdownPending;

        ///
        /// Initializes a new instance of the class.
        ///
        public AsyncFileAppender()
        {
            //initialize our queue
            EventLoggingQueue = new Queue<LoggingEvent>();
            //put the event initially in non-signalled state
            AppenderShutdownResetEvent = new ManualResetEvent(false);

            //start the async process of handling pending tasks
            if (!ShutdownPending)
            {
                Thread thread = new Thread(LogMessages);
                thread.Priority = ThreadPriority.Lowest;
                thread.IsBackground = true;
                thread.Start();
            }
        }

        ///
        /// Appends the specified logging events.
        ///
        /// The logging events.
        protected override void Append(LoggingEvent[] loggingEvents)
        {
            foreach (LoggingEvent loggingEvent in loggingEvents)
                Append(loggingEvent);
        }

        ///
        /// Appends the specified logging event.
        ///
        /// The logging event.
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (FilterEvent(loggingEvent))
                Enqueue(loggingEvent);
        }

        ///
        /// Logs the messages in a fixed time interval.
        ///
        private void LogMessages()
        {
            LoggingEvent loggingEvent;
            //we keep on processing tasks until shutdown on repository is called
            while (!ShutdownPending)
            {
                // wait for new events to fill up for a short amount of time, but not too short
                // so CPU is not wasted too much and disk writes are reduced
                System.Threading.Thread.Sleep(100);

                // write all events that added up in the queue
                while (Dequeue(out loggingEvent))
                    if (loggingEvent != null)
                        base.Append(loggingEvent);
            }

            //we are done with our logging, sent the signal to the parent thread
            //so that it can commence shut down
            AppenderShutdownResetEvent.Set();
        }

        ///
        /// Enqueues the specified logging event for asynchronous logging.
        ///
        /// The logging event.
        private void Enqueue(LoggingEvent loggingEvent)
        {
            lock (SyncRoot)
            {
                loggingEvent.Fix = FixFlags.All; // as suggested by tuangi
                EventLoggingQueue.Enqueue(loggingEvent);
            }
        }

        ///
        /// fetch the object at the beginning of the queue and removes it from the queue
        ///
        /// The logging event.
        ///
        private bool Dequeue(out LoggingEvent loggingEvent)
        {
            lock (SyncRoot)
            {
                if (EventLoggingQueue.Count > 0)
                {
                    loggingEvent = EventLoggingQueue.Dequeue();
                    return true;
                }
                else
                {
                    loggingEvent = null;
                    return false;
                }
            }
        }

        

        ///
        /// OnClose method is called, when the shut down of the repository is called
        ///
        protected override void OnClose()
        {

            //Flush the Queue
            Flush();

            //set the ShutdownPending flag to true, so that
            //AppendLoggingEvents would know it is time to wrap up
            //whatever it is doing
            ShutdownPending = true;

            //wait till we receive signal from manualResetEvent
            //which is signalled from AppendLoggingEvents
            AppenderShutdownResetEvent.WaitOne(TimeSpan.FromSeconds(10)); // you’ll have 10 seconds to clean up
            base.OnClose();
        }


        /// <summary>
        /// Flush the queue
        /// </summary>
        public void Flush()
        {
            lock (SyncRoot)
            {
                if (EventLoggingQueue.Count > 0)
                {
                    foreach (var loggingEvent in EventLoggingQueue)
                    {
                        base.Append(loggingEvent);
                    }

                    EventLoggingQueue.Clear();
                }
            }
        }
    }
}
