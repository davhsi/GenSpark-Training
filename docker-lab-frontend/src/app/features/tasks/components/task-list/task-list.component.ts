import { Component, input, output } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { TaskItem } from '../../models/task.model';
import { TaskCardComponent } from '../task-card/task-card.component';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [TaskCardComponent, DecimalPipe],
  template: `
    <div class="flex flex-col gap-8">
      <div>
        <h2 class="font-sans text-2xl font-medium tracking-tight text-white mb-2">Task Registry</h2>
        <p class="font-mono text-xs text-zinc-500">// {{ tasks().length | number:'2.0-0' }} TASKS DETECTED</p>
      </div>

      <div class="grid grid-cols-1 xl:grid-cols-2 gap-4">
        @for (task of tasks(); track task.id) {
          <app-task-card 
            [task]="task"
            (toggleStatus)="toggleStatus.emit($event)"
            (delete)="delete.emit($event)"
          />
        } @empty {
          <div class="xl:col-span-2 border border-dashed border-zinc-800 p-12 flex flex-col items-center justify-center text-center">
            <span class="font-mono text-sm text-zinc-500 mb-2">// NO TASKS FOUND</span>
            <p class="font-sans text-zinc-400">Initialize a new task using the control panel.</p>
          </div>
        }
      </div>
    </div>
  `
})
export class TaskListComponent {
  tasks = input.required<TaskItem[]>();
  toggleStatus = output<TaskItem>();
  delete = output<number>();
}
