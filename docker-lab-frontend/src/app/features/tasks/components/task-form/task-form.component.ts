import { Component, output, inject } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { CreateTaskItem } from '../../models/task.model';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="bg-zinc-950 border border-zinc-800 p-8 flex flex-col gap-6 relative h-full">
      <!-- Decorative element -->
      <div class="absolute top-0 left-0 w-2 h-full bg-emerald-500"></div>
      
      <div>
        <h2 class="font-sans text-2xl font-medium tracking-tight text-white mb-2">New Task</h2>
        <p class="font-mono text-xs text-zinc-500">// ENTER DETAILS BELOW</p>
      </div>

      <div class="flex flex-col gap-2">
        <label for="title" class="font-mono text-xs text-zinc-400 uppercase tracking-widest">Title</label>
        <input 
          id="title" 
          formControlName="title" 
          type="text" 
          class="bg-zinc-900 border border-zinc-800 text-white font-sans px-4 py-3 focus:outline-none focus:border-emerald-500 transition-colors"
          placeholder="Task title"
        />
        @if (form.get('title')?.invalid && form.get('title')?.touched) {
          <span class="font-mono text-xs text-red-500">TITLE IS REQUIRED</span>
        }
      </div>

      <div class="flex flex-col gap-2">
        <label for="description" class="font-mono text-xs text-zinc-400 uppercase tracking-widest">Description</label>
        <textarea 
          id="description" 
          formControlName="description" 
          rows="4"
          class="bg-zinc-900 border border-zinc-800 text-white font-sans px-4 py-3 focus:outline-none focus:border-emerald-500 transition-colors resize-none"
          placeholder="Optional description"
        ></textarea>
      </div>

      <button 
        type="submit" 
        [disabled]="form.invalid"
        class="mt-auto self-start bg-emerald-500 text-zinc-950 font-mono text-sm font-bold uppercase tracking-widest px-8 py-3 transition-transform active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed hover:bg-emerald-400 cursor-pointer"
      >
        Create Task
      </button>
    </form>
  `
})
export class TaskFormComponent {
  private fb = inject(FormBuilder);
  
  create = output<CreateTaskItem>();

  form = this.fb.group({
    title: ['', Validators.required],
    description: ['']
  });

  onSubmit() {
    if (this.form.valid) {
      this.create.emit({
        title: this.form.value.title!,
        description: this.form.value.description || '',
        isCompleted: false
      });
      this.form.reset();
    }
  }
}
