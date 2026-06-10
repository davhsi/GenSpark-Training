export class Product {
  id = 0;
  title = '';
  price = 0;
  rating = 0;
  thumbnail = '';
  description = '';
  brand = '';
  category = '';
}

export class ProductsResponse {
  products: Product[] = [];
}
