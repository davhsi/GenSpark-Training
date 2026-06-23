import { Component, OnInit, inject } from '@angular/core';
import { TaskStore } from './store/task.store';
import { TaskListComponent } from './components/task-list/task-list.component';
import { TaskFormComponent } from './components/task-form/task-form.component';
import { TaskItem, CreateTaskItem } from './models/task.model';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [TaskListComponent, TaskFormComponent],
  template: `
    <div class="bg-zinc-950 p-6 md:p-12 lg:p-24 selection:bg-emerald-500/30">
      
      <div class="max-w-[1400px] mx-auto">
        <!-- Header -->
        <header class="mb-16 border-b border-zinc-900 pb-8">
          <h1 class="font-sans text-4xl md:text-6xl tracking-tighter leading-none text-white mb-4">
            Operations
          </h1>
          <p class="font-mono text-sm text-zinc-500 uppercase tracking-widest">
            // SYS_STATUS: {{ store.loading() ? 'FETCHING' : 'ONLINE' }}
          </p>
          @if (store.error()) {
            <div class="mt-4 p-4 border border-red-500/30 bg-red-500/10 text-red-500 font-mono text-xs">
              [ERROR] {{ store.error() }}
            </div>
          }
        </header>

        <!-- Main Layout: Asymmetric Split -->
        <div class="grid grid-cols-1 lg:grid-cols-12 gap-12 lg:gap-24 items-start">
          <!-- Form Section: 4 columns -->
          <div class="lg:col-span-4 sticky top-12">
            <app-task-form (create)="onCreate($event)" />
          </div>

          <!-- List Section: 8 columns -->
          <div class="lg:col-span-8">
            <app-task-list 
              [tasks]="store.tasks()"
              (toggleStatus)="onToggleStatus($event)"
              (delete)="onDelete($event)"
            />
          </div>
        </div>
      </div>

    </div>
  `
})
export class TasksComponent implements OnInit {
  store = inject(TaskStore);

  ngOnInit() {
    this.store.loadTasks();
  }

  onCreate(task: CreateTaskItem) {
    this.store.addTask(task);
  }

  onToggleStatus(task: TaskItem) {
    this.store.updateTask({ ...task, isCompleted: !task.isCompleted });
  }

  onDelete(id: number) {
    this.store.deleteTask(id);
  }
}
