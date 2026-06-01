import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-customers',
  imports: [FormsModule],
  templateUrl: './customers.html',
  styleUrl: './customers.css',
})
export class Customers {

  customerName:string = "John Doe";
  color:string = "blue";

  handleChangeClick(){
    alert("Customer Name: " + this.customerName);
  }

}