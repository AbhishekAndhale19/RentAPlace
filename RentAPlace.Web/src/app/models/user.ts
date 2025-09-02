export interface User{
    id:string;
    fullName:string;
    email:string;
    accesstoken?:string;
    isOwner:boolean;    
}