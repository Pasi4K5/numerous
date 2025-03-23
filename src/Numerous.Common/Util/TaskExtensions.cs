// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Runtime.CompilerServices;

namespace Numerous.Common.Util;

public static class TaskExtensions
{
    public static TaskAwaiter GetAwaiter(this (Task, Task) tasks)
    {
        return TaskUtil.WhenAll(tasks.Item1, tasks.Item2).GetAwaiter();
    }

    public static TaskAwaiter GetAwaiter(this (Task, Task, Task) tasks)
    {
        return TaskUtil.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3).GetAwaiter();
    }

    public static TaskAwaiter<(T1, T2)> GetAwaiter<T1, T2>(this (Task<T1>, Task<T2>) tasks)
    {
        return CombineTasks().GetAwaiter();

        async Task<(T1, T2)> CombineTasks()
        {
            var (task1, task2) = tasks;
            await TaskUtil.WhenAll(task1, task2);

            return (task1.Result, task2.Result);
        }
    }
}
