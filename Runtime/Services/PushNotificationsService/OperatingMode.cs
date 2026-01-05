using System;

namespace TapEmpire.Services.Notifications

{
    [Flags]
    public enum OperatingMode
    {
        /// <summary>
        /// Do not perform any queueing at all. All notifications are scheduled with the operating system
        /// immediately.
        /// </summary>
        NoQueue = 0x00,

        /// <summary>
        /// <para>
        /// Queue messages that are scheduled with this manager.
        /// No messages will be sent to the operating system until the application is backgrounded.
        /// </para>
        /// <para>
        /// If badge numbers are not set, will automatically increment them. This will only happen if NO badge numbers
        /// for pending notifications are ever set.
        /// </para>
        /// </summary>
        Queue = 0x01,

        /// <summary>
        /// When the application is foregrounded, clear all pending notifications.
        /// </summary>
        ClearOnForegrounding = 0x02,

        /// <summary>
        /// After clearing events, will put future ones back into the queue if they are marked with <see cref="PendingNotification.Reschedule"/>.
        /// </summary>
        /// <remarks>
        /// Only valid if <see cref="ClearOnForegrounding"/> is also set.
        /// </remarks>
        RescheduleAfterClearing = 0x04,

        /// <summary>
        /// Combines the behaviour of <see cref="Queue"/> and <see cref="ClearOnForegrounding"/>.
        /// </summary>
        QueueAndClear = Queue | ClearOnForegrounding,

        /// <summary>
        /// <para>
        /// Combines the behaviour of <see cref="Queue"/>, <see cref="ClearOnForegrounding"/> and
        /// <see cref="RescheduleAfterClearing"/>.
        /// </para>
        /// <para>
        /// Ensures that messages will never be displayed while the application is in the foreground.
        /// </para>
        /// </summary>
        QueueClearAndReschedule = Queue | ClearOnForegrounding | RescheduleAfterClearing,
    }
}