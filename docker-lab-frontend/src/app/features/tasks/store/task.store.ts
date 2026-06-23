import { Injectable, signal, computed, inject } from '@angular/core';
import { TaskApi } from '../api/task.api';
import { TaskItem, CreateTaskItem, UpdateTaskItem } from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskStore {
  private readonly api = inject(TaskApi);

  // State
  private readonly _tasks = signal<TaskItem[]>([]);
  private readonly _loading = signal<boolean>(false);
  private readonly _error = signal<string | null>(null);

  // Selectors
  readonly tasks = computed(() => this._tasks());
  readonly loading = computed(() => this._loading());
  readonly error = computed(() => this._error());

  // Actions
  loadTasks(): void {
    this._loading.set(true);
    this._error.set(null);
    this.api.getAllTasks().subscribe({
      next: (tasks) => {
        this._tasks.set(tasks);
        this._loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load tasks', err);
        this._error.set('Failed to load tasks. Ensure backend is running.');
        this._loading.set(false);
      }
    });
  }

  addTask(task: CreateTaskItem): void {
    this._loading.set(true);
    this.api.createTask(task).subscribe({
      next: (newTask) => {
        this._tasks.update((tasks) => [...tasks, newTask]);
        this._loading.set(false);
      },
      error: (err) => {
        console.error('Failed to add task', err);
        this._error.set('Failed to add task');
        this._loading.set(false);
      }
    });
  }

  updateTask(task: UpdateTaskItem): void {
    this._loading.set(true);
    this.api.updateTask(task.id, task).subscribe({
      next: () => {
        this._tasks.update((tasks) =>
          tasks.map((t) => (t.id === task.id ? task : t))
        );
        this._loading.set(false);
      },
      error: (err) => {
        console.error('Failed to update task', err);
        this._error.set('Failed to update task');
        this._loading.set(false);
      }
    });
  }

  deleteTask(id: number): void {
    this._loading.set(true);
    this.api.deleteTask(id).subscribe({
      next: () => {
        this._tasks.update((tasks) => tasks.filter((t) => t.id !== id));
        this._loading.set(false);
      },
      error: (err) => {
        console.error('Failed to delete task', err);
        this._error.set('Failed to delete task');
        this._loading.set(false);
      }
    });
  }
}
