const axios = require('axios');

const BASE_URL = 'http://localhost:5241';
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

let token = '';

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

async function debugEmployeesEndpoint() {
    try {
        console.log(`\n🔍 DEBUG: Šta vraća GET /api/zaposleni...`);
        const response = await axios.get(`${BASE_URL}/api/zaposleni`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log(`✅ API poziv uspešan!`);
        console.log(`\n📋 POTPUNA STRUKTURA ODGOVORA:`);
        console.log(`==============================================`);
        console.log(JSON.stringify(response.data, null, 2));
        console.log(`==============================================`);
        
        console.log(`\n🔍 ANALIZA:`);
        console.log(`Type: ${typeof response.data}`);
        console.log(`Is Array: ${Array.isArray(response.data)}`);
        
        if (Array.isArray(response.data)) {
            console.log(`Length: ${response.data.length}`);
        } else {
            console.log(`Keys: ${Object.keys(response.data)}`);
        }
        
        return true;
    } catch (error) {
        console.log(`❌ Greška: ${error.message}`);
        if (error.response) {
            console.log(`Status: ${error.response.status}`);
            console.log(`Response:`, error.response.data);
        }
        return false;
    }
}

async function runDebug() {
    console.log('🚨 POTPUNI DEBUG TEST - Šta vraća API za zaposlene?\n');
    
    const loginOK = await login();
    if (!loginOK) return;
    
    await debugEmployeesEndpoint();
}

runDebug().catch(console.error);