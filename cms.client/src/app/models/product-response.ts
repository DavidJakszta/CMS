export interface ProductResponse {
  id: number;
  name: string;
  price: number;
  description: string;
  pictureUrl?: string;
  ownerId: number;
  ownerName: string;
}
