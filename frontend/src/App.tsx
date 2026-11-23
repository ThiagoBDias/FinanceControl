import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "./contexts/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import AppLayout from "./layouts/AppLayout";
import Login from "./pages/Login/Login";
import Register from "./pages/Register/Register";
import Dashboard from "./pages/Dashboard/Dashboard";
import Accounts from "./pages/Accounts/Accounts";
import Transactions from "./pages/Transactions/Transactions";
import Categories from "./pages/Categories/Categories";
import Cards from "./pages/Cards/Cards";
import Invoices from "./pages/Invoices/Invoices";
import Loading from "./components/Loading";
import Debts from "./pages/Debts/Debts";
import Budgets from "./pages/Budgets/Budgets";

export default function App(){
 const {session, loading}=useAuth();
 if (loading) return <Loading />;
 return <Routes>
  <Route path="/login" element={session?<Navigate to="/" replace/>:<Login/>}/>
  <Route path="/register" element={session?<Navigate to="/" replace/>:<Register/>}/>
  <Route element={<ProtectedRoute/>}><Route element={<AppLayout/>}>
    <Route index element={<Navigate to="/dashboard" replace/>}/><Route path="/dashboard" element={<Dashboard/>}/><Route path="/accounts" element={<Accounts/>}/><Route path="/transactions" element={<Transactions/>}/>
    <Route path="/categories" element={<Categories/>}/><Route path="/cards" element={<Cards/>}/><Route path="/invoices" element={<Invoices/>}/><Route path="/budgets" element={<Budgets/>}/><Route path="/debts" element={<Debts/>}/>
  </Route></Route>
  <Route path="*" element={<Navigate to={session?"/":"/login"} replace/>}/>
 </Routes>;
}
