const axios = require('axios');

const BASE_URL = 'http://localhost:5241';
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

let token = '';

const formatTime = () => {
    const now = new Date();
    return now.toLocaleTimeString('sr-RS', { 
        hour: '2-digit', 
        minute: '2-digit', 
        second: '2-digit' 
    });
};

async function login() {
    try {
        console.log(`🔐 Login...`);
        const response = await axios.post(`${BASE_URL}/api/auth/login`, {
            username: 'admin',
            password: 'admin123'
        });
        
        token = response.data.Token || response.data.token;
        console.log(`✅ Login uspešan!`);
        return true;
    } catch (error) {
        console.log(`❌ Login greška: ${error.message}`);
        return false;
    }
}

async function testGetAllEmployees() {
    try {
        console.log(`\n👥 Testiram GET /api/zaposleni (za dropdown)...`);
        const response = await axios.get(`${BASE_URL}/api/zaposleni`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log(`[${formatTime()}] ✅ Uspešno dohvaćeno ${response.data.length} zaposlenih`);
        
        if (response.data.length > 0) {
            console.log(`\n📋 Lista zaposlenih za dropdown:`);
            response.data.slice(0, 5).forEach((emp, index) => {
                console.log(`  ${index + 1}. ${emp.ime} ${emp.prezime} (ID: ${emp.id})`);
            });
            
            if (response.data.length > 5) {
                console.log(`  ... i još ${response.data.length - 5} zaposlenih`);
            }
        } else {
            console.log(`❌ PROBLEM: Lista je prazna!`);
        }
        
        return true;
    } catch (error) {
        console.log(`[${formatTime()}] ❌ Greška pri dohvatanju svih zaposlenih:`);
        console.log(`Status: ${error.response?.status}`);
        console.log(`Poruka: ${error.response?.data?.message || error.message}`);
        return false;
    }
}

async function runTest() {
    console.log('🔍 TEST: Da li API za zaposlene radi (za dropdown)?\n');
    
    const loginOK = await login();
    if (!loginOK) return;
    
    await testGetAllEmployees();
}

runTest().catch(console.error);